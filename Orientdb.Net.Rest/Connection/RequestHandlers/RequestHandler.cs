using System;
using System.IO;
using Orientdb.Net.Connection.RequestState;
using Orientdb.Net.ConnectionPool;
using Orientdb.Net.Exceptions;
using Orientdb.Net.Providers;
using Orientdb.Net.Serialization;

namespace Orientdb.Net.Connection.RequestHandlers
{
    internal class RequestHandler : RequestHandlerBase
    {
        public RequestHandler(
            IConnectionConfigurationValues settings,
            IConnectionPool connectionPool,
            IConnection connection,
            IOrientdbSerializer serializer,
            IMemoryStreamProvider memoryStreamProvider,
            ITransportDelegator delegator)
            : base(settings, connection, connectionPool, serializer, memoryStreamProvider, delegator)
        {
        }

        public OrientdbResponse<T> Request<T>(TransportRequestState<T> requestState, object data = null)
        {
            byte[] postData = PostData(data);
            requestState.TickSerialization(postData);

            OrientdbResponse<T> response = DoRequest(requestState);
            requestState.SetResult(response);
            return response;
        }

        private OrientdbResponse<T> SelectNextNode<T>(TransportRequestState<T> requestState)
        {
            // Select the next node to hit and signal whether the selected node needs a ping
            bool nodeRequiresPinging = _delegator.SelectNextNode(requestState);
            if (!nodeRequiresPinging) return null;

            bool pingSuccess = _delegator.Ping(requestState);
            // If ping is not successful retry request on the next node the connection pool selects
            return !pingSuccess ? RetryRequest(requestState) : null;
        }

        private OrientdbResponse<Stream> DoOrientdbCall<T>(TransportRequestState<T> requestState)
        {
            // Do the actual request by calling into IConnection
            // We wrap it in a IRequestTimings to audit the request
            OrientdbResponse<Stream> streamResponse;
            using (IRequestTimings requestAudit = requestState.InitiateRequest(RequestType.OrientdbCall))
            {
                streamResponse = CallInToConnection(requestState);
                requestAudit.Finish(streamResponse.Success, streamResponse.HttpStatusCode);
            }
            return streamResponse;
        }

        private OrientdbResponse<T> ReturnStreamOrVoidResponse<T>(
            TransportRequestState<T> requestState, OrientdbResponse<Stream> streamResponse)
        {
            // If the response never recieved a status code and has a caught exception make sure we throw it
            if (streamResponse.HttpStatusCode.GetValueOrDefault(-1) <= 0 && streamResponse.OriginalException != null)
                throw streamResponse.OriginalException;

            // If the user explicitly wants a stream returned the undisposed stream
            if (typeof (Stream).IsAssignableFrom(typeof (T)))
                return streamResponse as OrientdbResponse<T>;

            if (!typeof (VoidResponse).IsAssignableFrom(typeof (T))) return null;

            OrientdbResponse<VoidResponse> voidResponse = OrientdbResponse.CloneFrom<VoidResponse>(streamResponse, null);

            return voidResponse as OrientdbResponse<T>;
        }

        private OrientdbResponse<T> ReturnTypedResponse<T>(
            TransportRequestState<T> requestState,
            OrientdbResponse<Stream> streamResponse,
            out OrientdbServerError error)
        {
            error = null;

            // Read to memory stream if needed
            bool hasResponse = streamResponse.Response != null;
            bool forceRead = _settings.KeepRawResponse || typeof (T) == typeof (string) || typeof (T) == typeof (byte[]);
            byte[] bytes = null;
            if (hasResponse && forceRead)
            {
                MemoryStream ms = _memoryStreamProvider.New();
                streamResponse.Response.CopyTo(ms);
                bytes = ms.ToArray();
                streamResponse.Response.Close();
                streamResponse.Response = ms;
                streamResponse.Response.Position = 0;
            }
            // Set rawresponse if needed
            if (_settings.KeepRawResponse) streamResponse.ResponseRaw = bytes;

            bool isValidResponse = IsValidResponse(requestState, streamResponse);
            if (isValidResponse)
                return StreamToTypedResponse<T>(streamResponse, requestState, bytes);

            // If error read error 
            error = GetErrorFromStream<T>(streamResponse.Response);
            OrientdbResponse<T> typedResponse = OrientdbResponse.CloneFrom(streamResponse, default(T));
            SetStringOrByteResult(typedResponse, bytes);
            return typedResponse;
        }

        private OrientdbResponse<T> CoordinateRequest<T>(TransportRequestState<T> requestState, int maxRetries,
            int retried, ref bool aliveResponse)
        {
            OrientdbResponse<T> pingRetryRequest = SelectNextNode(requestState);
            if (pingRetryRequest != null) return pingRetryRequest;

            OrientdbResponse<Stream> streamResponse = DoOrientdbCall(requestState);

            aliveResponse = streamResponse.SuccessOrKnownError;

            if (!DoneProcessing(streamResponse, requestState, maxRetries, retried))
                return null;

            OrientdbServerError error = null;
            OrientdbResponse<T> typedResponse = ReturnStreamOrVoidResponse(requestState, streamResponse)
                                                ?? ReturnTypedResponse(requestState, streamResponse, out error);

            OptionallyCloseResponseStreamAndSetSuccess(requestState, error, typedResponse, streamResponse);
            if (error != null && _settings.ThrowOnOrientdbServerExceptions)
                throw new OrientdbServerException(error);
            return typedResponse;
        }

        private OrientdbResponse<T> DoRequest<T>(TransportRequestState<T> requestState)
        {
            OrientdbResponse<T> sniffAuthResponse = TrySniffOnStaleClusterState(requestState);
            if (sniffAuthResponse != null) return sniffAuthResponse;

            bool aliveResponse = false;
            bool seenError = false;
            int retried = requestState.Retried;
            int maxRetries = _delegator.GetMaximumRetries(requestState.RequestConfiguration);

            try
            {
                OrientdbResponse<T> response = CoordinateRequest(requestState, maxRetries, retried, ref aliveResponse);
                if (response != null) return response;
            }
            catch (OrientdbAuthenticationException e)
            {
                return HandleAuthenticationException(requestState, e);
            }
            catch (MaxRetryException)
            {
                //TODO ifdef ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            catch (OrientdbServerException)
            {
                //TODO ifdef ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            catch (Exception e)
            {
                requestState.SeenExceptions.Add(e);
                if (!requestState.UsingPooling || maxRetries == 0 && retried == 0)
                {
                    //TODO ifdef ExceptionDispatchInfo.Capture(ex).Throw();
                    throw;
                }
                seenError = true;
                return RetryRequest(requestState);
            }
            finally
            {
                //make sure we always call markalive on the uri if the connection was succesful
                if (!seenError && aliveResponse)
                    _connectionPool.MarkAlive(requestState.CurrentNode);
            }
            return RetryRequest(requestState);
        }

        private OrientdbResponse<T> RetryRequest<T>(TransportRequestState<T> requestState)
        {
            int maxRetries = _delegator.GetMaximumRetries(requestState.RequestConfiguration);

            _connectionPool.MarkDead(requestState.CurrentNode, _settings.DeadTimeout, _settings.MaxDeadTimeout);

            try
            {
                _delegator.SniffOnConnectionFailure(requestState);
            }
            catch (OrientdbAuthenticationException e)
            {
                //If the sniff already returned a 401 fail/return a response as early as possible
                return HandleAuthenticationException(requestState, e);
            }

            ThrowMaxRetryExceptionWhenNeeded(requestState, maxRetries);

            return DoRequest(requestState);
        }

        private OrientdbResponse<Stream> CallInToConnection<T>(TransportRequestState<T> requestState)
        {
            Uri uri = requestState.CreatePathOnCurrentNode();
            byte[] postData = requestState.PostData;
            IRequestConfiguration requestConfiguration = requestState.RequestConfiguration;
            switch (requestState.Method.ToLowerInvariant())
            {
                case "post":
                    return _connection.PostSync(uri, postData, requestConfiguration);
                case "put":
                    return _connection.PutSync(uri, postData, requestConfiguration);
                case "head":
                    return _connection.HeadSync(uri, requestConfiguration);
                case "get":
                    return _connection.GetSync(uri, requestConfiguration);
                case "delete":
                    return postData == null || postData.Length == 0
                        ? _connection.DeleteSync(uri, requestConfiguration)
                        : _connection.DeleteSync(uri, postData, requestConfiguration);
            }
            throw new Exception("Unknown HTTP method " + requestState.Method);
        }

        protected OrientdbResponse<T> StreamToTypedResponse<T>(
            OrientdbResponse<Stream> streamResponse,
            ITransportRequestState requestState,
            byte[] readBytes
            )
        {
            //set response
            if (typeof (T) == typeof (string) || typeof (T) == typeof (byte[]))
            {
                OrientdbResponse<T> clone = OrientdbResponse.CloneFrom(streamResponse, default(T));
                SetStringOrByteResult(clone, readBytes);
                return clone;
            }
            OrientdbResponse<T> typedResponse = OrientdbResponse.CloneFrom(streamResponse, default(T));
            using (streamResponse.Response)
            {
                Func<IOrientdbResponse, Stream, object> deserializationState = requestState.ResponseCreationOverride;
                var customConverter = deserializationState as Func<IOrientdbResponse, Stream, T>;
                if (customConverter != null)
                {
                    T t = customConverter(typedResponse, streamResponse.Response);
                    typedResponse.Response = t;
                    return typedResponse;
                }
                var deserialized = _serializer.Deserialize<T>(streamResponse.Response);
                typedResponse.Response = deserialized;
                return typedResponse;
            }
        }
    }
}