using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Orientdb.Net.ConnectionPool;
using Orientdb.Net.Purify;

namespace Orientdb.Net.Connection.RequestState
{
    public class TransportRequestState<T> : IDisposable, ITransportRequestState
    {
        private readonly bool _metricsEnabled;

        private readonly Stopwatch _stopwatch;
        private readonly bool _traceEnabled;

        private Uri _currentNode;
        private OrientdbResponse<T> _result;

        public TransportRequestState(
            IConnectionConfigurationValues settings,
            IRequestParameters requestParameters,
            string method,
            string path)
        {
            StartedOn = DateTime.UtcNow;
            SeenNodes = new List<Uri>();
            SeenExceptions = new List<Exception>();
            ClientSettings = settings;
            RequestParameters = requestParameters;
            _traceEnabled = settings.TraceEnabled;
            _metricsEnabled = settings.MetricsEnabled;
            if (_metricsEnabled || _traceEnabled)
                _stopwatch = Stopwatch.StartNew();

            Method = method;
            Path = path;
            if (RequestParameters != null)
            {
                if (RequestParameters.QueryString != null)
                    Path +=
                        RequestParameters.QueryString.ToNameValueCollection(ClientSettings.Serializer).ToQueryString();
                ResponseCreationOverride = RequestParameters.DeserializationState;
            }
        }

        public IConnectionConfigurationValues ClientSettings { get; private set; }

        public IRequestParameters RequestParameters { get; private set; }

        public string Method { get; private set; }

        public string Path { get; private set; }

        public byte[] PostData { get; private set; }
        public int Pings { get; set; }
        public int Sniffs { get; set; }

        public List<Uri> SeenNodes { get; private set; }
        public long SerializationTime { get; private set; }
        public long RequestEndTime { get; private set; }
        public long DeserializationTime { get; private set; }

        public void Dispose()
        {
            if (!_traceEnabled || _result == null)
                return;

            if (_result.Success)
            {
                Trace.TraceInformation("OrientDB {0} {1} ({2}):\r\n{3}"
                    , _result.RequestMethod
                    , _result.RequestUrl
                    , _stopwatch.Elapsed
                    , _result
                    );
            }
            else
            {
                Trace.TraceError(
                    "OrientDB {0} {1} ({2}):\r\n{3}"
                    , _result.RequestMethod
                    , _result.RequestUrl
                    , _stopwatch.Elapsed
                    , _result.ToString()
                    );
            }
        }

        public IRequestConfiguration RequestConfiguration
        {
            get { return RequestParameters == null ? null : RequestParameters.RequestConfiguration; }
        }

        public bool UsingPooling
        {
            get
            {
                IConnectionPool pool = ClientSettings.ConnectionPool;
                return pool != null && pool.GetType() != typeof (SingleNodeConnectionPool);
            }
        }

        public int? Seed { get; set; }

        public Func<IOrientdbResponse, Stream, object> ResponseCreationOverride { get; set; }

        public bool SniffedOnConnectionFailure { get; set; }

        public int Retried
        {
            get { return Math.Max(0, SeenNodes.Count - 1); }
        }

        public List<Exception> SeenExceptions { get; private set; }
        public List<RequestMetrics> RequestMetrics { get; set; }

        public Uri CurrentNode
        {
            get
            {
                return RequestConfiguration != null && RequestConfiguration.ForceNode != null
                    ? RequestConfiguration.ForceNode
                    : _currentNode;
            }
            set
            {
                _currentNode = value;
                SeenNodes.Add(value);
            }
        }

        public DateTime StartedOn { get; private set; }


        public IRequestTimings InitiateRequest(RequestType requestType)
        {
            if (!ClientSettings.MetricsEnabled)
                return NoopRequestTimings.Instance;

            if (RequestMetrics == null) RequestMetrics = new List<RequestMetrics>();
            return new RequestTimings(requestType, CurrentNode, Path, RequestMetrics);
        }

        public Uri CreatePathOnCurrentNode(string path = null)
        {
            IConnectionConfigurationValues s = ClientSettings;
            path = path ?? Path;
            if (s.QueryStringParameters != null)
            {
                var tempUri = new Uri(CurrentNode, path);
                string qs = s.QueryStringParameters.ToQueryString(tempUri.Query.IsNullOrEmpty() ? "?" : "&");
                path += qs;
            }
            Uri uri = path.IsNullOrEmpty() ? CurrentNode : new Uri(CurrentNode, path);
            return uri.Purify();
        }

        public void TickSerialization(byte[] postData)
        {
            PostData = postData;
            if (_metricsEnabled)
                SerializationTime = _stopwatch.ElapsedMilliseconds;
        }

        public void SetResult(OrientdbResponse<T> result)
        {
            if (result == null)
            {
                if (_stopwatch != null) _stopwatch.Stop();
                return;
            }
            result.NumberOfRetries = Retried;
            if (ClientSettings.MetricsEnabled)
                result.Metrics = new CallMetrics
                {
                    Path = Path,
                    SerializationTime = SerializationTime,
                    DeserializationTime = DeserializationTime,
                    StartedOn = StartedOn,
                    CompletedOn = DateTime.UtcNow,
                    Requests = RequestMetrics
                };

            //TODO this forced set is done in several places, shouldn't this be enough?
            var objectNeedsResponseRef = result.Response as IResponseWithRequestInformation;
            if (objectNeedsResponseRef != null) objectNeedsResponseRef.RequestInformation = result;

            if (ClientSettings.ConnectionStatusHandler != null)
                ClientSettings.ConnectionStatusHandler(result);

            if (_stopwatch != null) _stopwatch.Stop();

            if (!_traceEnabled) return;

            _result = result;
        }
    }
}