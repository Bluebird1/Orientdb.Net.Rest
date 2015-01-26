using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orientdb.Net.Connection.RequestState;
using Orientdb.Net.ConnectionPool;
using Orientdb.Net.Exceptions;
using Orientdb.Net.Providers;
using Orientdb.Net.Serialization;

namespace Orientdb.Net.Connection.RequestHandlers
{
    internal class RequestHandlerBase
    {
        protected const int BufferSize = 4096;
        protected static readonly string MaxRetryExceptionMessage = "Failed after retrying {2} times: '{0} {1}'. {3}";

        protected static readonly string TookTooLongExceptionMessage =
            "Retry timeout {4} was hit after retrying {2} times: '{0} {1}'. {3}";

        protected static readonly string MaxRetryInnerMessage =
            "InnerException: {0}, InnerMessage: {1}, InnerStackTrace: {2}";

        protected readonly IConnection _connection;
        protected readonly IConnectionPool _connectionPool;
        protected readonly ITransportDelegator _delegator;
        protected readonly IMemoryStreamProvider _memoryStreamProvider;
        protected readonly IOrientdbSerializer _serializer;
        protected readonly IConnectionConfigurationValues _settings;

        protected readonly bool _throwMaxRetry;

        protected RequestHandlerBase(
            IConnectionConfigurationValues settings,
            IConnection connection,
            IConnectionPool connectionPool,
            IOrientdbSerializer serializer,
            IMemoryStreamProvider memoryStreamProvider,
            ITransportDelegator delegator)
        {
            _settings = settings;
            _connection = connection;
            _connectionPool = connectionPool;
            _serializer = serializer;
            _memoryStreamProvider = memoryStreamProvider;
            _delegator = delegator;

            _throwMaxRetry = !(_connectionPool is SingleNodeConnectionPool);
        }

        protected byte[] PostData(object data)
        {
            if (data == null) return null;

            var bytes = data as byte[];
            if (bytes != null) return bytes;

            var s = data as string;
            if (s != null) return s.Utf8Bytes();

            var ss = data as IEnumerable<string>;
            if (ss != null) return (string.Join("\n", ss) + "\n").Utf8Bytes();

            var so = data as IEnumerable<object>;
            if (so == null) return _serializer.Serialize(data);
            string joined = string.Join("\n", so
                .Select(soo => _serializer.Serialize(soo, SerializationFormatting.None).Utf8String())) + "\n";
            return joined.Utf8Bytes();
        }

        protected static bool IsValidResponse(ITransportRequestState requestState, IOrientdbResponse streamResponse)
        {
            return streamResponse.Success
                   || StatusCodeAllowed(requestState.RequestConfiguration, streamResponse.HttpStatusCode);
        }

        protected static bool StatusCodeAllowed(IRequestConfiguration requestConfiguration, int? statusCode)
        {
            if (requestConfiguration == null)
                return false;

            return requestConfiguration.AllowedStatusCodes.HasAny(i => i == statusCode);
        }

        protected bool TypeOfResponseCopiesDirectly<T>()
        {
            Type type = typeof (T);
            return type == typeof (string) || type == typeof (byte[]) || typeof (Stream).IsAssignableFrom(typeof (T));
        }

        protected bool SetStringOrByteResult<T>(OrientdbResponse<T> original, byte[] bytes)
        {
            Type type = typeof (T);
            if (type == typeof (string))
            {
                SetStringResult(original as OrientdbResponse<string>, bytes);
                return true;
            }
            if (type == typeof (byte[]))
            {
                SetByteResult(original as OrientdbResponse<byte[]>, bytes);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Determines whether the stream response is our final stream response:
        ///     IF response is success or known error
        ///     OR maxRetries is 0 and retried is 0 (maxRetries could change in between retries to 0)
        ///     AND sniff on connection fault does not find more nodes (causing maxRetry to grow)
        ///     AND maxretries is no retried
        /// </summary>
        protected bool DoneProcessing<T>(
            OrientdbResponse<Stream> streamResponse,
            TransportRequestState<T> requestState,
            int maxRetries,
            int retried)
        {
            return (streamResponse != null && streamResponse.SuccessOrKnownError)
                   || (maxRetries == 0
                       && retried == 0
                       && !_delegator.SniffOnFaultDiscoveredMoreNodes(requestState, retried, streamResponse)
                       );
        }

        protected void ThrowMaxRetryExceptionWhenNeeded<T>(TransportRequestState<T> requestState, int maxRetries)
        {
            bool tookToLong = _delegator.TookTooLongToRetry(requestState);

            //not out of date and we havent depleted our retries, get the hell out of here
            if (!tookToLong && requestState.Retried < maxRetries) return;

            List<Exception> innerExceptions = requestState.SeenExceptions.Where(e => e != null).ToList();
            Exception innerException = !innerExceptions.HasAny()
                ? null
                : (innerExceptions.Count() == 1)
                    ? innerExceptions.First()
                    : new AggregateException(requestState.SeenExceptions);

            //When we are not using pooling we forcefully rethrow the exception
            //and never wrap it in a maxretry exception 
            if (!requestState.UsingPooling && innerException != null)
                throw innerException;

            string exceptionMessage = tookToLong
                ? CreateTookTooLongExceptionMessage(requestState, innerException)
                : CreateMaxRetryExceptionMessage(requestState, innerException);
            throw new MaxRetryException(exceptionMessage, innerException);
        }

        protected string CreateInnerExceptionMessage<T>(TransportRequestState<T> requestState, Exception e)
        {
            if (e == null) return null;
            var aggregate = e as AggregateException;
            if (aggregate == null)
                return "\r\n" + MaxRetryInnerMessage.F(e.GetType().Name, e.Message, e.StackTrace);
            aggregate = aggregate.Flatten();
            List<string> innerExceptions = aggregate.InnerExceptions
                .Select(ae => MaxRetryInnerMessage.F(ae.GetType().Name, ae.Message, ae.StackTrace))
                .ToList();
            return "\r\n" + string.Join("\r\n", innerExceptions);
        }

        protected string CreateMaxRetryExceptionMessage<T>(TransportRequestState<T> requestState, Exception e)
        {
            string innerException = CreateInnerExceptionMessage(requestState, e);
            string exceptionMessage = MaxRetryExceptionMessage
                .F(requestState.Method, requestState.Path, requestState.Retried, innerException);
            return exceptionMessage;
        }

        protected string CreateTookTooLongExceptionMessage<T>(TransportRequestState<T> requestState, Exception e)
        {
            string innerException = CreateInnerExceptionMessage(requestState, e);
            TimeSpan timeout = _settings.MaxRetryTimeout.GetValueOrDefault(TimeSpan.FromMilliseconds(_settings.Timeout));
            string exceptionMessage = TookTooLongExceptionMessage
                .F(requestState.Method, requestState.Path, requestState.Retried, innerException, timeout);
            return exceptionMessage;
        }

        protected void OptionallyCloseResponseStreamAndSetSuccess<T>(
            ITransportRequestState requestState,
            OrientdbServerError error,
            OrientdbResponse<T> typedResponse,
            OrientdbResponse<Stream> streamResponse)
        {
            if (streamResponse.Response != null && !typeof (Stream).IsAssignableFrom(typeof (T)))
                streamResponse.Response.Close();

            if (error != null)
            {
                typedResponse.Success = false;
                if (typedResponse.OriginalException == null)
                    typedResponse.OriginalException = new OrientdbServerException(error);
            }

            //TODO UNIT TEST OR BEGONE
            if (!typedResponse.Success
                && requestState.RequestConfiguration != null
                && requestState.RequestConfiguration.AllowedStatusCodes.HasAny(i => i == streamResponse.HttpStatusCode))
            {
                typedResponse.Success = true;
            }
        }

        protected void SetStringResult(OrientdbResponse<string> response, byte[] rawResponse)
        {
            response.Response = rawResponse.Utf8String();
        }

        protected void SetByteResult(OrientdbResponse<byte[]> response, byte[] rawResponse)
        {
            response.Response = rawResponse;
        }

        protected OrientdbServerError GetErrorFromStream<T>(Stream stream)
        {
            try
            {
                var e = _serializer.Deserialize<OneToOneServerException>(stream);
                return OrientdbServerError.Create(e);
            }
                // ReSharper disable once EmptyGeneralCatchClause
                // parsing failure of exception should not be fatal, its a best case helper.
            catch
            {
            }
            return null;
        }


        /// <summary>
        ///     Sniffs when the cluster state is stale, when sniffing returns a 401 return a response for T to return directly
        /// </summary>
        protected OrientdbResponse<T> TrySniffOnStaleClusterState<T>(TransportRequestState<T> requestState)
        {
            try
            {
                //If connectionSettings is configured to sniff periodically, sniff when stale.
                _delegator.SniffOnStaleClusterState(requestState);
                return null;
            }
            catch (OrientdbAuthenticationException e)
            {
                return HandleAuthenticationException(requestState, e);
            }
        }

        protected OrientdbResponse<T> HandleAuthenticationException<T>(TransportRequestState<T> requestState,
            OrientdbAuthenticationException exception)
        {
            if (requestState.ClientSettings.ThrowOnOrientdbServerExceptions)
                throw exception.ToElasticsearchServerException();

            OrientdbResponse<T> response = OrientdbResponse.CloneFrom(exception.Response, default(T));
            response.Request = requestState.PostData;
            response.RequestUrl = requestState.Path;
            response.RequestMethod = requestState.Method;
            return response;
        }
    }
}