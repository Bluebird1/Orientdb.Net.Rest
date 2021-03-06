﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Orientdb.Net.Connection.RequestState;
using Orientdb.Net.ConnectionPool;
using Orientdb.Net.Exceptions;
using Orientdb.Net.Providers;
using Orientdb.Net.Serialization;

namespace Orientdb.Net.Connection.RequestHandlers
{
    internal class RequestHandlerAsync : RequestHandlerBase
    {
        private class ReadResponse<T>
        {
            public byte[] Bytes { get; set; }
            public OrientdbResponse<T> Response { get; set; }
            public OrientdbServerError Error { get; set; }
        }

        public RequestHandlerAsync(
            IConnectionConfigurationValues settings,
            IConnectionPool connectionPool,
            IConnection connection,
            IOrientdbSerializer serializer,
            IMemoryStreamProvider memoryStreamProvider,
            ITransportDelegator delegator)
            : base(settings, connection, connectionPool, serializer, memoryStreamProvider, delegator)
        {
        }

        public Task<OrientdbResponse<T>> RequestAsync<T>(TransportRequestState<T> requestState, object data = null)
        {
            // Serialize request and inform requestState so it can keep track of serialization times
            var bytes = PostData(data);
            requestState.TickSerialization(bytes);

            return this.DoRequestAsync<T>(requestState)
                // When the request returns again inform the request state so it can do its bookkeeping
                .ContinueWith(t => this.SetResultOnRequestState(t, requestState))
                .Unwrap();
        }

        private Task<OrientdbResponse<T>> DoRequestAsync<T>(TransportRequestState<T> requestState)
        {
            var sniffAuthResponse = this.TrySniffOnStaleClusterState(requestState);
            if (sniffAuthResponse != null) return this.ReturnCompletedTaskFor(sniffAuthResponse);

            // Select the next node to hit and signal wheter the selected node needs a ping
            var uriRequiresPing = this._delegator.SelectNextNode(requestState);
            if (uriRequiresPing)
            {
                // First branch into a ping call and then handle the ping response
                return this._delegator.PingAsync(requestState)
                    // Handle ping response will do the actual call if the ping is valid
                    .ContinueWith(t => this.HandlePingResponse(t, requestState))
                    .Unwrap();
            }
            // Perform call and retry if necessary
            return FinishOrRetryRequestAsync(requestState);
        }

        private Task<OrientdbResponse<T>> HandlePingResponse<T>(Task<bool> t, TransportRequestState<T> requestState)
        {
            // If ping is not faulted and completed do the actual call
            if (!t.IsFaulted) return t.IsCompleted ? this.FinishOrRetryRequestAsync(requestState) : null;

            // Ping resulted in an exception 
            if (t.Exception == null)
                return this.RetryRequestAsync(requestState);

            // Keep track of the exception we just saw, t.Exception is a flattened AggregateException
            requestState.SeenExceptions.Add(t.Exception.InnerException);

            // If the ping exception was that of an unauthorized exception, 
            var authenticationException = t.Exception.InnerException as OrientdbAuthenticationException;
            if (authenticationException != null)
            {
                var tcs = new TaskCompletionSource<OrientdbResponse<T>>();
                this.SetAuthenticationExceptionOnRequestState(requestState, authenticationException, tcs);
                return tcs.Task;
            }

            return this.RetryRequestAsync(requestState);
        }

        private Task<OrientdbResponse<T>> FinishOrRetryRequestAsync<T>(TransportRequestState<T> requestState)
        {
            var rq = requestState.InitiateRequest(RequestType.OrientdbCall);
            return CallIntoConnectionAsync(requestState)
                .ContinueWith(t => HandleStreamResponse(t, rq, requestState))
                .Unwrap();
        }

        private Task<ReadResponse<T>> ReturnVoidResponse<T>(OrientdbResponse<Stream> streamResponse)
        {
            streamResponse.Response.Close();
            var voidResponse = OrientdbResponse.CloneFrom<VoidResponse>(streamResponse, null);
            return this.ReturnCompletedTaskFor(new ReadResponse<T>() { Response = voidResponse as OrientdbResponse<T> });
        }

        private Task<ReadResponse<T>> ReturnTypedResponse<T>(OrientdbResponse<Stream> streamResponse, TransportRequestState<T> requestState)
        {
            // Read to memory stream if needed
            Task<Stream> getStream = null;
            var response = new ReadResponse<T>();
            var hasResponse = streamResponse.Response != null;
            var forceRead = this._settings.KeepRawResponse || typeof(T) == typeof(string) || typeof(T) == typeof(byte[]);
            if (hasResponse && forceRead)
            {
                var memoryStream = this._memoryStreamProvider.New();
                getStream = this.Iterate(this.ReadStreamAsync(streamResponse.Response, memoryStream), memoryStream)
                    .ContinueWith(streamReadTask =>
                    {
                        response.Bytes = streamReadTask.Result.ToArray();
                        streamResponse.Response.Close();
                        streamReadTask.Result.Position = 0;
                        return streamReadTask.Result as Stream;
                    });
            }
            else
            {
                getStream = this.ReturnCompletedTaskFor(streamResponse.Response);
            }

            return getStream.ContinueWith(delegate(Task<Stream> gotStream)
            {
                var isValidResponse = IsValidResponse(requestState, streamResponse);
                var typedResponse = OrientdbResponse.CloneFrom<T>(streamResponse, default(T));
                if (!isValidResponse)
                {
                    response.Error = GetErrorFromStream<T>(gotStream.Result);
                    this.SetStringOrByteResult(typedResponse, response.Bytes);
                    if (gotStream.Result != null) gotStream.Result.Close();
                    response.Response = typedResponse;
                    return this.ReturnCompletedTaskFor(response);
                }
                if (this.SetStringOrByteResult(typedResponse, response.Bytes))
                {
                    if (gotStream.Result != null) gotStream.Result.Close();
                    response.Response = typedResponse;
                    return this.ReturnCompletedTaskFor(response);
                }
                return this.DeserializeAsyncToResponse<T>(gotStream.Result, requestState, typedResponse, response);
            }).Unwrap();
        }

        private Task<OrientdbResponse<T>> SetResultOnRequestState<T>(Task<OrientdbResponse<T>> t, TransportRequestState<T> requestState)
        {
            var tcs = new TaskCompletionSource<OrientdbResponse<T>>();
            if (t.Exception != null)
            {
                var authenticationException = t.Exception.InnerException as OrientdbAuthenticationException;

                if (authenticationException != null)
                {
                    this.SetAuthenticationExceptionOnRequestState(requestState, authenticationException, tcs);
                }
                else
                {
                    tcs.SetException(t.Exception.Flatten());
                    requestState.SetResult(null);
                }
            }
            else
            {
                tcs.SetResult(t.Result);
                requestState.SetResult(t.Result);
            }
            return tcs.Task;
        }

        protected void SetAuthenticationExceptionOnRequestState<T>(
            TransportRequestState<T> requestState,
            OrientdbAuthenticationException exception,
            TaskCompletionSource<OrientdbResponse<T>> tcs)
        {
            var result = this.HandleAuthenticationException(requestState, exception);
            tcs.SetResult(result);
            requestState.SetResult(result);
        }

        private Task<OrientdbResponse<T>> HandleStreamResponse<T>(Task<OrientdbResponse<Stream>> t, IRequestTimings rq, TransportRequestState<T> requestState)
        {
            if (t.IsFaulted)
            {
                requestState.SeenExceptions.Add(t.Exception);
            }
            var streamResponse = !t.IsFaulted ? t.Result : null;
            //var streamResponse = t.Result;
            // Audit the call into connection straight away
            rq.Finish(streamResponse != null && streamResponse.Success, streamResponse == null ? -1 : streamResponse.HttpStatusCode);
            rq.Dispose();

            //If we are not using any pooling and we see an exception we rethrow
            //regardless whether maxretry is set.
            if (!requestState.UsingPooling && t.IsFaulted && t.Exception != null)
                throw t.Exception;

            // Figure out the maximum number of retries, this might 
            var maxRetries = this._delegator.GetMaximumRetries(requestState.RequestConfiguration);

            var retried = requestState.Retried;

            // If we haven't recieved a successful response and we are not yet done processing the stream attempt a retry
            if (t.Status != TaskStatus.RanToCompletion || !this.DoneProcessing(streamResponse, requestState, maxRetries, retried))
                return this.RetryRequestAsync<T>(requestState);

            // If the response never recieved a status code and has a caught exception make sure we throw it
            if (streamResponse.HttpStatusCode.GetValueOrDefault(-1) <= 0 && streamResponse.OriginalException != null)
                throw streamResponse.OriginalException;

            // If the user explicitly wants a stream return the undisposed stream
            if (typeof(Stream).IsAssignableFrom(typeof(T)))
                return this.ReturnResponseAsTask<T>(streamResponse);

            var readResponseTask = (typeof(VoidResponse).IsAssignableFrom(typeof(T)))
                ? this.ReturnVoidResponse<T>(streamResponse)
                : this.ReturnTypedResponse(streamResponse, requestState);

            return readResponseTask.ContinueWith(gotTypedResponse =>
            {
                var r = gotTypedResponse.Result;
                if (this._settings.KeepRawResponse) r.Response.ResponseRaw = r.Bytes;
                this.OptionallyCloseResponseStreamAndSetSuccess(requestState, r.Error, r.Response, t.Result);
                if (r.Error != null && this._settings.ThrowOnOrientdbServerExceptions)
                    throw new OrientdbServerException(r.Error);
                if (r.Response.SuccessOrKnownError)
                    this._connectionPool.MarkAlive(requestState.CurrentNode);
                return r.Response;
            });
        }

        private Task<T> ReturnCompletedTaskFor<T>(T result)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(result);
            return tcs.Task;
        }

        private Task<OrientdbResponse<T>> ReturnResponseAsTask<T>(IOrientdbResponse response)
        {
            var tcs = new TaskCompletionSource<OrientdbResponse<T>>();
            tcs.SetResult(response as OrientdbResponse<T>);
            return tcs.Task;
        }

        private Task<OrientdbResponse<T>> RetryRequestAsync<T>(TransportRequestState<T> requestState)
        {
            var maxRetries = this._delegator.GetMaximumRetries(requestState.RequestConfiguration);

            this._connectionPool.MarkDead(requestState.CurrentNode, this._settings.DeadTimeout, this._settings.MaxDeadTimeout);

            this._delegator.SniffOnConnectionFailure(requestState);

            this.ThrowMaxRetryExceptionWhenNeeded(requestState, maxRetries);

            return this.DoRequestAsync<T>(requestState);
        }

        private Task<OrientdbResponse<Stream>> CallIntoConnectionAsync<T>(TransportRequestState<T> requestState)
        {
            var uri = requestState.CreatePathOnCurrentNode();
            var postData = requestState.PostData;
            var requestConfiguration = requestState.RequestConfiguration;
            var method = requestState.Method.ToLowerInvariant();
            try
            {
                switch (method)
                {
                    case "head": return this._connection.Head(uri, requestConfiguration);
                    case "get": return this._connection.Get(uri, requestConfiguration);
                    case "post": return this._connection.Post(uri, postData, requestConfiguration);
                    case "put": return this._connection.Put(uri, postData, requestConfiguration);
                    case "delete":
                        return postData == null || postData.Length == 0
                            ? this._connection.Delete(uri, requestConfiguration)
                            : this._connection.Delete(uri, postData, requestConfiguration);
                    default:
                        throw new Exception("Unknown HTTP method " + requestState.Method);
                }
            }
            catch (Exception e)
            {
                var tcs = new TaskCompletionSource<OrientdbResponse<Stream>>();
                tcs.SetException(e);
                return tcs.Task;
            }
        }

        private Task<MemoryStream> Iterate(IEnumerable<Task> asyncIterator, MemoryStream memoryStream)
        {
            var tcs = new TaskCompletionSource<MemoryStream>();
            var enumerator = asyncIterator.GetEnumerator();
            Action<Task> recursiveBody = null;
            recursiveBody = completedTask =>
            {
                if (completedTask != null && completedTask.IsFaulted)
                {
                    var exception = completedTask.Exception.InnerException;
                    tcs.TrySetException(exception);
                    enumerator.Dispose();
                }
                else if (enumerator.MoveNext())
                {
                    enumerator.Current.ContinueWith(recursiveBody);
                }
                else
                {
                    tcs.SetResult(memoryStream);
                    enumerator.Dispose();
                }
            };
            recursiveBody(null);
            return tcs.Task;
        }

        private Task<ReadResponse<T>> DeserializeAsyncToResponse<T>(
            Stream response,
            ITransportRequestState requestState,
            OrientdbResponse<T> typedResponse,
            ReadResponse<T> readResponse)
        {
            var tcs = new TaskCompletionSource<ReadResponse<T>>();
            var responseInstantiater = requestState.ResponseCreationOverride;
            var customConverter = responseInstantiater as Func<IOrientdbResponse, Stream, T>;
            if (customConverter == null)
            {
                return this._serializer.DeserializeAsync<T>(response)
                    .ContinueWith(t =>
                    {
                        typedResponse.Response = t.Result;
                        readResponse.Response = typedResponse;
                        return readResponse;
                    });
            }
            using (response)
            {
                var t = customConverter(typedResponse, response);
                typedResponse.Response = t;
                readResponse.Response = typedResponse;
                tcs.SetResult(readResponse);
                return tcs.Task;
            }
        }

        private IEnumerable<Task<MemoryStream>> ReadStreamAsync(Stream responseStream, MemoryStream memoryStream)
        {
            var buffer = new byte[BufferSize];
            try
            {
                while (responseStream != null)
                {
                    var read = Task<int>.Factory.FromAsync(responseStream.BeginRead, responseStream.EndRead, buffer, 0, BufferSize, null);
                    yield return read.ContinueWith(t => memoryStream);
                    if (read.Result == 0) break;
                    memoryStream.Write(buffer, 0, read.Result);
                }
            }
            finally
            {
                if (responseStream != null)
                    responseStream.Dispose();
            }
        }
    }
}