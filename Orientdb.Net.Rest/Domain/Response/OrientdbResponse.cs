using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Orientdb.Net.Connection;
using Orientdb.Net.ConnectionPool;
using Orientdb.Net.Serialization;
using System.Runtime.CompilerServices;

// ReSharper disable CheckNamespace

namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public static class OrientdbResponse
    {
        internal static Task<OrientdbResponse<DynamicDictionary>> WrapAsync(
            Task<OrientdbResponse<Dictionary<string, object>>> responseTask)
        {
            return responseTask
                .ContinueWith(t =>
                {
                    if (t.IsFaulted && t.Exception != null)
                        throw t.Exception.Flatten().InnerException;

                    return ToDynamicResponse(t.Result);
                });
        }

        internal static OrientdbResponse<DynamicDictionary> Wrap(OrientdbResponse<Dictionary<string, object>> response)
        {
            return ToDynamicResponse(response);
        }

        public static OrientdbResponse<TTo> CloneFrom<TTo>(IOrientdbResponse from, TTo to)
        {
            var response = new OrientdbResponse<TTo>(from.Settings)
            {
                OriginalException = from.OriginalException,
                HttpStatusCode = from.HttpStatusCode,
                Request = from.Request,
                RequestMethod = from.RequestMethod,
                RequestUrl = from.RequestUrl,
                Response = to,
                ResponseRaw = from.ResponseRaw,
                Serializer = from.Settings.Serializer,
                Settings = from.Settings,
                Success = from.Success,
                Metrics = from.Metrics
            };
            var tt = to as IResponseWithRequestInformation;
            if (tt != null)
                tt.RequestInformation = response;
            return response;
        }

        private static OrientdbResponse<DynamicDictionary> ToDynamicResponse(
            OrientdbResponse<Dictionary<string, object>> response)
        {
            return CloneFrom(response, response.Response != null ? DynamicDictionary.Create(response.Response) : null);
        }
    }

    public class OrientdbResponse<T> : IOrientdbResponse
    {
        private static readonly string _printFormat;
        private static readonly string _errorFormat;

        static OrientdbResponse()
        {
            _printFormat = "StatusCode: {1}, {0}\tMethod: {2}, {0}\tUrl: {3}, {0}\tRequest: {4}, {0}\tResponse: {5}";
            _errorFormat = "{0}\tExceptionMessage: {1}{0}\t StackTrace: {2}";
        }

        protected internal OrientdbResponse(IConnectionConfigurationValues settings)
        {
            Settings = settings;
            Serializer = settings.Serializer;
        }

        private OrientdbResponse(IConnectionConfigurationValues settings, Exception e)
            : this(settings)
        {
            Success = false;
            OriginalException = e;
        }

        private OrientdbResponse(IConnectionConfigurationValues settings, int statusCode)
            : this(settings)
        {
            Success = statusCode >= 200 && statusCode < 300;
            HttpStatusCode = statusCode;
        }

        /// <summary>
        ///     This property returns the mapped elasticsearch server exception
        /// </summary>
        public OrientdbServerError ServerError
        {
            get
            {
                var esException = OriginalException as OrientdbServerException;
                if (esException == null) return null;
                return new OrientdbServerError
                {
                    Error = esException.Message,
                    ExceptionType = esException.ExceptionType,
                    Status = esException.Status
                };
            }
        }

        public T Response { get; protected internal set; }
        public IOrientdbSerializer Serializer { get; protected internal set; }

        /// <summary>
        ///     If the response is succesful or has a known error (400-500 range)
        ///     The client should not retry this call
        /// </summary>
        public bool SuccessOrKnownError
        {
            get
            {
                IConnectionPool pool = Settings.ConnectionPool;
                bool usingPool = pool != null && pool.GetType() != typeof (SingleNodeConnectionPool);

                return Success ||
                       (!usingPool && HttpStatusCode.GetValueOrDefault(1) < 0)
                       || (HttpStatusCode.HasValue
                           && HttpStatusCode.Value != 503 //service unavailable needs to be retried
                           && HttpStatusCode.Value != 502 //bad gateway needs to be retried 
                           && ((HttpStatusCode.Value >= 400 && HttpStatusCode.Value < 599)));
            }
        }

        public bool Success { get; protected internal set; }

        public Exception OriginalException { get; protected internal set; }

        public string RequestMethod { get; protected internal set; }

        public string RequestUrl { get; protected internal set; }

        public IConnectionConfigurationValues Settings { get; protected internal set; }

        public byte[] Request { get; protected internal set; }

        public int NumberOfRetries { get; protected internal set; }
        public CallMetrics Metrics { get; protected internal set; }

        /// <summary>
        ///     The raw byte response, only set when IncludeRawResponse() is set on Connection configuration
        /// </summary>
        public byte[] ResponseRaw { get; protected internal set; }

        public int? HttpStatusCode { get; protected internal set; }

        public static OrientdbResponse<T> CreateError(IConnectionConfigurationValues settings, Exception e,
            string method, string path, byte[] request)
        {
            var cs = new OrientdbResponse<T>(settings, e) {Request = request, RequestUrl = path, RequestMethod = method};
            return cs;
        }

        public static OrientdbResponse<T> Create(
            IConnectionConfigurationValues settings, int statusCode, string method, string path, byte[] request,
            Exception innerException = null)
        {
            var cs = new OrientdbResponse<T>(settings, statusCode)
            {
                Request = request,
                RequestUrl = path,
                RequestMethod = method,
                OriginalException = innerException
            };
            return cs;
        }

        public static OrientdbResponse<T> Create(
            IConnectionConfigurationValues settings, int statusCode, string method, string path, byte[] request,
            T response, Exception innerException = null)
        {
            var cs = new OrientdbResponse<T>(settings, statusCode)
            {
                Request = request,
                RequestUrl = path,
                RequestMethod = method,
                Response = response,
                OriginalException = innerException
            };
            return cs;
        }

        public override string ToString()
        {
            OrientdbResponse<T> r = this;
            Exception e = r.OriginalException;
            string response =
                "<Response stream not captured or already read to completion by serializer, set ExposeRawResponse() on connectionsettings to force it to be set on>";
            if (typeof (T) == typeof (string))
                response = Response as string;
            else if (Settings.KeepRawResponse)
                response = ResponseRaw.Utf8String();
            else if (typeof (T) == typeof (byte[]))
                response = (Response as byte[]).Utf8String();

            string requestJson = null;

            if (r.Request != null)
            {
                requestJson = r.Request.Utf8String();
            }

            string print = _printFormat.F(
                Environment.NewLine,
                r.HttpStatusCode.HasValue ? r.HttpStatusCode.Value.ToString(CultureInfo.InvariantCulture) : "-1",
                r.RequestMethod,
                r.RequestUrl,
                requestJson,
                response
                );
            if (!Success && e != null)
            {
                print += _errorFormat.F(Environment.NewLine, e.Message, e.StackTrace);
            }
            return print;
        }
    }
}