using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Orientdb.Net.Connection
{
    public class HttpConnection : IConnection
    {
        private const int BUFFER_SIZE = 1024;

        private readonly bool _enableTrace;
        private readonly Semaphore _resourceLock;

        static HttpConnection()
        {
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 10000;

            if (Type.GetType("Mono.Runtime") == null)
                HttpWebRequest.DefaultMaximumErrorResponseLength = -1;
        }

        public HttpConnection(IConnectionConfigurationValues settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            ConnectionSettings = settings;
            if (settings.MaximumAsyncConnections > 0)
            {
                int semaphore = Math.Max(1, settings.MaximumAsyncConnections);
                _resourceLock = new Semaphore(semaphore, semaphore);
            }
            _enableTrace = settings.TraceEnabled;
        }

        protected IConnectionConfigurationValues ConnectionSettings { get; set; }

        public virtual OrientdbResponse<Stream> GetSync(Uri uri, IRequestConfiguration requestSpecificConfig = null)
        {
            return HeaderOnlyRequest(uri, "GET", requestSpecificConfig);
        }

        public virtual OrientdbResponse<Stream> HeadSync(Uri uri, IRequestConfiguration requestSpecificConfig = null)
        {
            return HeaderOnlyRequest(uri, "HEAD", requestSpecificConfig);
        }

        public virtual OrientdbResponse<Stream> PostSync(Uri uri, byte[] data,
            IRequestConfiguration requestSpecificConfig = null)
        {
            return BodyRequest(uri, data, "POST", requestSpecificConfig);
        }

        public virtual OrientdbResponse<Stream> PutSync(Uri uri, byte[] data,
            IRequestConfiguration requestSpecificConfig = null)
        {
            return BodyRequest(uri, data, "PUT", requestSpecificConfig);
        }

        public virtual OrientdbResponse<Stream> DeleteSync(Uri uri, IRequestConfiguration requestSpecificConfig = null)
        {
            return HeaderOnlyRequest(uri, "DELETE", requestSpecificConfig);
        }

        public virtual OrientdbResponse<Stream> DeleteSync(Uri uri, byte[] data,
            IRequestConfiguration requestSpecificConfig = null)
        {
            return BodyRequest(uri, data, "DELETE", requestSpecificConfig);
        }


        public virtual Task<OrientdbResponse<Stream>> Get(Uri uri, IRequestConfiguration requestSpecificConfig = null)
        {
            HttpWebRequest r = CreateHttpWebRequest(uri, "GET", null, requestSpecificConfig);
            return DoAsyncRequest(r, requestSpecificConfig: requestSpecificConfig);
        }

        public virtual Task<OrientdbResponse<Stream>> Head(Uri uri, IRequestConfiguration requestSpecificConfig = null)
        {
            HttpWebRequest r = CreateHttpWebRequest(uri, "HEAD", null, requestSpecificConfig);
            return DoAsyncRequest(r, requestSpecificConfig: requestSpecificConfig);
        }

        public virtual Task<OrientdbResponse<Stream>> Post(Uri uri, byte[] data,
            IRequestConfiguration requestSpecificConfig = null)
        {
            HttpWebRequest r = CreateHttpWebRequest(uri, "POST", data, requestSpecificConfig);
            return DoAsyncRequest(r, data, requestSpecificConfig);
        }

        public virtual Task<OrientdbResponse<Stream>> Put(Uri uri, byte[] data,
            IRequestConfiguration requestSpecificConfig = null)
        {
            HttpWebRequest r = CreateHttpWebRequest(uri, "PUT", data, requestSpecificConfig);
            return DoAsyncRequest(r, data, requestSpecificConfig);
        }

        public virtual Task<OrientdbResponse<Stream>> Delete(Uri uri, byte[] data,
            IRequestConfiguration requestSpecificConfig = null)
        {
            HttpWebRequest r = CreateHttpWebRequest(uri, "DELETE", data, requestSpecificConfig);
            return DoAsyncRequest(r, data, requestSpecificConfig);
        }

        public virtual Task<OrientdbResponse<Stream>> Delete(Uri uri, IRequestConfiguration requestSpecificConfig = null)
        {
            HttpWebRequest r = CreateHttpWebRequest(uri, "DELETE", null, requestSpecificConfig);
            return DoAsyncRequest(r, requestSpecificConfig: requestSpecificConfig);
        }

        private OrientdbResponse<Stream> HeaderOnlyRequest(Uri uri, string method,
            IRequestConfiguration requestSpecificConfig)
        {
            HttpWebRequest r = CreateHttpWebRequest(uri, method, null, requestSpecificConfig);
            return DoSynchronousRequest(r, requestSpecificConfig: requestSpecificConfig);
        }

        private OrientdbResponse<Stream> BodyRequest(Uri uri, byte[] data, string method,
            IRequestConfiguration requestSpecificConfig)
        {
            HttpWebRequest r = CreateHttpWebRequest(uri, method, data, requestSpecificConfig);
            return DoSynchronousRequest(r, data, requestSpecificConfig);
        }

        private static void ThreadTimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                var request = state as HttpWebRequest;
                if (request != null)
                {
                    request.Abort();
                }
            }
        }


        protected virtual HttpWebRequest CreateHttpWebRequest(Uri uri, string method, byte[] data,
            IRequestConfiguration requestSpecificConfig)
        {
            HttpWebRequest request = CreateWebRequest(uri, method, data, requestSpecificConfig);
            SetBasicAuthenticationIfNeeded(uri, request, requestSpecificConfig);
            SetProxyIfNeeded(request);
            return request;
        }

        private void SetProxyIfNeeded(HttpWebRequest myReq)
        {
            if (!string.IsNullOrEmpty(ConnectionSettings.ProxyAddress))
            {
                var proxy = new WebProxy();
                var uri = new Uri(ConnectionSettings.ProxyAddress);
                var credentials = new NetworkCredential(ConnectionSettings.ProxyUsername,
                    ConnectionSettings.ProxyPassword);
                proxy.Address = uri;
                proxy.Credentials = credentials;
                myReq.Proxy = proxy;
            }

            if (ConnectionSettings.DisableAutomaticProxyDetection)
            {
                myReq.Proxy = null;
            }
        }

        private void SetBasicAuthenticationIfNeeded(Uri uri, HttpWebRequest request,
            IRequestConfiguration requestSpecificConfig)
        {
            // Basic auth credentials take the following precedence (highest -> lowest):
            // 1 - Specified on the request (highest precedence)
            // 2 - Specified at the global IConnectionSettings level
            // 3 - Specified with the URI (lowest precedence)

            string userInfo = Uri.UnescapeDataString(uri.UserInfo);

            if (ConnectionSettings.BasicAuthorizationCredentials != null)
                userInfo = ConnectionSettings.BasicAuthorizationCredentials.ToString();

            if (requestSpecificConfig != null && requestSpecificConfig.BasicAuthenticationCredentials != null)
                userInfo = requestSpecificConfig.BasicAuthenticationCredentials.ToString();

            if (!userInfo.IsNullOrEmpty())
                request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(userInfo));
        }

        protected virtual HttpWebRequest CreateWebRequest(Uri uri, string method, byte[] data,
            IRequestConfiguration requestSpecificConfig)
        {
            var request = (HttpWebRequest) WebRequest.Create(uri);
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.MaximumResponseHeadersLength = -1;
            request.Pipelined = ConnectionSettings.HttpPipeliningEnabled
                                || (requestSpecificConfig != null && requestSpecificConfig.EnableHttpPipelining);

            if (ConnectionSettings.EnableCompressedResponses)
            {
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.Headers.Add("Accept-Encoding", "gzip,deflate");
            }

            if (requestSpecificConfig != null && !string.IsNullOrWhiteSpace(requestSpecificConfig.ContentType))
            {
                request.Accept = requestSpecificConfig.ContentType;
                request.ContentType = requestSpecificConfig.ContentType;
            }

            int timeout = GetRequestTimeout(requestSpecificConfig);
            request.Timeout = timeout;
            request.ReadWriteTimeout = timeout;
            request.Method = method;

            //WebRequest won't send Content-Length: 0 for empty bodies
            //which goes against RFC's and might break i.e IIS when used as a proxy.
            //see: https://github.com/elasticsearch/elasticsearch-net/issues/562
            string m = method.ToLowerInvariant();
            if (m != "head" && m != "get" && (data == null || data.Length == 0))
                request.ContentLength = 0;

            return request;
        }

        protected virtual OrientdbResponse<Stream> DoSynchronousRequest(HttpWebRequest request, byte[] data = null,
            IRequestConfiguration requestSpecificConfig = null)
        {
            string path = request.RequestUri.ToString();
            string method = request.Method;

            if (data != null)
            {
                using (Stream r = request.GetRequestStream())
                {
                    r.Write(data, 0, data.Length);
                }
            }
            try
            {
                //http://msdn.microsoft.com/en-us/library/system.net.httpwebresponse.getresponsestream.aspx
                //Either the stream or the response object needs to be closed but not both although it won't
                //throw any errors if both are closed atleast one of them has to be Closed.
                //Since we expose the stream we let closing the stream determining when to close the connection
                var response = (HttpWebResponse) request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                return WebToOrientdbResponse(data, responseStream, response, method, path);
            }
            catch (WebException webException)
            {
                return HandleWebException(data, webException, method, path);
            }
        }

        private OrientdbResponse<Stream> HandleWebException(byte[] data, WebException webException, string method,
            string path)
        {
            OrientdbResponse<Stream> cs;
            var httpEx = webException.Response as HttpWebResponse;
            if (httpEx != null)
            {
                //StreamReader ms = new StreamReader(httpEx.GetResponseStream());
                //var response = ms.ReadToEnd();
                cs = WebToOrientdbResponse(data, httpEx.GetResponseStream(), httpEx, method, path);
                return cs;
            }
            cs = OrientdbResponse<Stream>.CreateError(ConnectionSettings, webException, method, path, data);
            return cs;
        }

        private OrientdbResponse<Stream> WebToOrientdbResponse(byte[] data, Stream responseStream,
            HttpWebResponse response, string method, string path)
        {
            OrientdbResponse<Stream> cs = OrientdbResponse<Stream>.Create(ConnectionSettings,
                (int) response.StatusCode, method, path, data);
            cs.Response = responseStream;
            return cs;
        }

        protected virtual Task<OrientdbResponse<Stream>> DoAsyncRequest(HttpWebRequest request, byte[] data = null,
            IRequestConfiguration requestSpecificConfig = null)
        {
            var tcs = new TaskCompletionSource<OrientdbResponse<Stream>>();
            if (ConnectionSettings.MaximumAsyncConnections <= 0
                || _resourceLock == null)
                return CreateIterateTask(request, data, requestSpecificConfig, tcs);

            int timeout = GetRequestTimeout(requestSpecificConfig);
            string path = request.RequestUri.ToString();
            string method = request.Method;
            if (!_resourceLock.WaitOne(timeout))
            {
                string m = "Could not start the operation before the timeout of " + timeout +
                           "ms completed while waiting for the semaphore";
                OrientdbResponse<Stream> cs = OrientdbResponse<Stream>.CreateError(ConnectionSettings,
                    new TimeoutException(m), method,
                    path, data);
                tcs.SetResult(cs);
                return tcs.Task;
            }
            try
            {
                return CreateIterateTask(request, data, requestSpecificConfig, tcs);
            }
            finally
            {
                _resourceLock.Release();
            }
        }

        private Task<OrientdbResponse<Stream>> CreateIterateTask(HttpWebRequest request, byte[] data,
            IRequestConfiguration requestSpecificConfig, TaskCompletionSource<OrientdbResponse<Stream>> tcs)
        {
            Iterate(request, data, _AsyncSteps(request, tcs, data, requestSpecificConfig), tcs);
            return tcs.Task;
        }

        private IEnumerable<Task> _AsyncSteps(HttpWebRequest request, TaskCompletionSource<OrientdbResponse<Stream>> tcs,
            byte[] data, IRequestConfiguration requestSpecificConfig)
        {
            int timeout = GetRequestTimeout(requestSpecificConfig);

            if (data != null)
            {
                Task<Stream> getRequestStream = Task.Factory.FromAsync<Stream>(request.BeginGetRequestStream,
                    request.EndGetRequestStream, null);
                ThreadPool.RegisterWaitForSingleObject((getRequestStream as IAsyncResult).AsyncWaitHandle,
                    ThreadTimeoutCallback, request, timeout, true);
                yield return getRequestStream;

                Stream requestStream = getRequestStream.Result;
                try
                {
                    Task writeToRequestStream = Task.Factory.FromAsync(requestStream.BeginWrite, requestStream.EndWrite,
                        data, 0, data.Length, null);
                    yield return writeToRequestStream;
                }
                finally
                {
                    requestStream.Close();
                }
            }

            // Get the response
            Task<WebResponse> getResponse = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse,
                request.EndGetResponse, null);
            ThreadPool.RegisterWaitForSingleObject((getResponse as IAsyncResult).AsyncWaitHandle, ThreadTimeoutCallback,
                request, timeout, true);
            yield return getResponse;

            string path = request.RequestUri.ToString();
            string method = request.Method;

            //http://msdn.microsoft.com/en-us/library/system.net.httpwebresponse.getresponsestream.aspx
            //Either the stream or the response object needs to be closed but not both (although it won't)
            //throw any errors if both are closed atleast one of them has to be Closed.
            //Since we expose the stream we let closing the stream determining when to close the connection
            var response = (HttpWebResponse) getResponse.Result;
            Stream responseStream = response.GetResponseStream();
            OrientdbResponse<Stream> cs = OrientdbResponse<Stream>.Create(ConnectionSettings, (int) response.StatusCode,
                method, path,
                data);
            cs.Response = responseStream;
            tcs.TrySetResult(cs);
        }

        private void Iterate(HttpWebRequest request, byte[] data, IEnumerable<Task> asyncIterator,
            TaskCompletionSource<OrientdbResponse<Stream>> tcs)
        {
            IEnumerator<Task> enumerator = asyncIterator.GetEnumerator();
            Action<Task> recursiveBody = null;
            recursiveBody = completedTask =>
            {
                if (completedTask != null && completedTask.IsFaulted)
                {
                    //none of the individual steps in _AsyncSteps run in parallel for 1 request
                    //as this would be impossible we can assume Aggregate Exception.InnerException
                    Exception exception = completedTask.Exception.InnerException;

                    //cleanly exit from exceptions in stages if the exception is a webexception
                    if (exception is WebException)
                    {
                        string path = request.RequestUri.ToString();
                        string method = request.Method;
                        OrientdbResponse<Stream> response = HandleWebException(data, exception as WebException, method,
                            path);
                        tcs.SetResult(response);
                    }
                    else
                        tcs.TrySetException(exception);
                    enumerator.Dispose();
                }
                else if (enumerator.MoveNext())
                {
                    enumerator.Current.ContinueWith(recursiveBody, TaskContinuationOptions.ExecuteSynchronously);
                }
                else enumerator.Dispose();
            };
            recursiveBody(null);
        }

        private int GetRequestTimeout(IRequestConfiguration requestConfiguration)
        {
            if (requestConfiguration != null && requestConfiguration.ConnectTimeout.HasValue)
                return requestConfiguration.RequestTimeout.Value;

            return ConnectionSettings.Timeout;
        }
    }
}