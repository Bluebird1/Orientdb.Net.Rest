using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Orientdb.Net.Connection.Security;
using Orientdb.Net.ConnectionPool;
using Orientdb.Net.Serialization;

// ReSharper disable CheckNamespace

namespace Orientdb.Net.Connection
// ReSharper restore CheckNamespace
{
    public class ConnectionConfiguration :
        ConnectionConfiguration<ConnectionConfiguration>,
        IConnectionConfiguration<ConnectionConfiguration>
    {
        /// <summary>
        ///     ConnectionConfiguration allows you to control how ElasticsearchClient behaves and where/how it connects
        ///     to elasticsearch
        /// </summary>
        /// <param name="uri">The root of the elasticsearch node we want to connect to. Defaults to http://localhost:9200</param>
        public ConnectionConfiguration(Uri uri = null)
            : base(uri)
        {
        }

        /// <summary>
        ///     ConnectionConfiguration allows you to control how ElasticsearchClient behaves and where/how it connects
        ///     to elasticsearch
        /// </summary>
        /// <param name="connectionPool">A connection pool implementation that'll tell the client what nodes are available</param>
        public ConnectionConfiguration(IConnectionPool connectionPool)
            : base(connectionPool)
        {
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ConnectionConfiguration<T> : IConnectionConfigurationValues, IHideObjectMembers
        where T : ConnectionConfiguration<T>
    {
        private readonly IConnectionPool _connectionPool;

        IConnectionPool IConnectionConfigurationValues.ConnectionPool
        {
            get { return _connectionPool; }
        }

        private int _timeout;

        int IConnectionConfigurationValues.Timeout
        {
            get { return _timeout; }
        }

        private int? _pingTimeout;

        int? IConnectionConfigurationValues.PingTimeout
        {
            get { return _pingTimeout; }
        }

        private int? _deadTimeout;

        int? IConnectionConfigurationValues.DeadTimeout
        {
            get { return _deadTimeout; }
        }

        private int? _maxDeadTimeout;

        int? IConnectionConfigurationValues.MaxDeadTimeout
        {
            get { return _maxDeadTimeout; }
        }

        private TimeSpan? _maxRetryTimeout;

        TimeSpan? IConnectionConfigurationValues.MaxRetryTimeout
        {
            get { return _maxRetryTimeout; }
        }

        private string _proxyUsername;

        string IConnectionConfigurationValues.ProxyUsername
        {
            get { return _proxyUsername; }
        }

        private string _proxyPassword;

        string IConnectionConfigurationValues.ProxyPassword
        {
            get { return _proxyPassword; }
        }

        private bool _disablePings;

        bool IConnectionConfigurationValues.DisablePings
        {
            get { return _disablePings; }
        }

        private string _proxyAddress;

        string IConnectionConfigurationValues.ProxyAddress
        {
            get { return _proxyAddress; }
        }

        private bool _usePrettyResponses;

        bool IConnectionConfigurationValues.UsesPrettyResponses
        {
            get { return _usePrettyResponses; }
        }

#if DEBUG
        private bool _keepRawResponse = true;
#else
		private bool _keepRawResponse = false;
#endif

        bool IConnectionConfigurationValues.KeepRawResponse
        {
            get { return _keepRawResponse; }
        }

#if DEBUG
        private bool _enableMetrics = true;
#else
		private bool _enableMetrics = false;
#endif

        bool IConnectionConfigurationValues.MetricsEnabled
        {
            get { return _enableMetrics; }
        }

        private bool _disableAutomaticProxyDetection;

        bool IConnectionConfigurationValues.DisableAutomaticProxyDetection
        {
            get { return _disableAutomaticProxyDetection; }
        }

        private int _maximumAsyncConnections;

        int IConnectionConfigurationValues.MaximumAsyncConnections
        {
            get { return _maximumAsyncConnections; }
        }

        private int? _maxRetries;

        int? IConnectionConfigurationValues.MaxRetries
        {
            get { return _maxRetries; }
        }

        private bool _sniffOnStartup;

        bool IConnectionConfigurationValues.SniffsOnStartup
        {
            get { return _sniffOnStartup; }
        }

        private bool _sniffOnConectionFault;

        bool IConnectionConfigurationValues.SniffsOnConnectionFault
        {
            get { return _sniffOnConectionFault; }
        }

        private TimeSpan? _sniffLifeSpan;

        TimeSpan? IConnectionConfigurationValues.SniffInformationLifeSpan
        {
            get { return _sniffLifeSpan; }
        }

        private bool _compressionEnabled;

        bool IConnectionConfigurationValues.EnableCompressedResponses
        {
            get { return _compressionEnabled; }
        }

        private bool _traceEnabled;

        bool IConnectionConfigurationValues.TraceEnabled
        {
            get { return _traceEnabled; }
        }

        private bool _httpPipeliningEnabled;

        bool IConnectionConfigurationValues.HttpPipeliningEnabled
        {
            get { return _httpPipeliningEnabled; }
        }

        private bool _throwOnServerExceptions;

        bool IConnectionConfigurationValues.ThrowOnOrientdbServerExceptions
        {
            get { return _throwOnServerExceptions; }
        }

        private Action<IOrientdbResponse> _connectionStatusHandler;

        Action<IOrientdbResponse> IConnectionConfigurationValues.ConnectionStatusHandler
        {
            get { return _connectionStatusHandler; }
        }

        private NameValueCollection _queryString;

        NameValueCollection IConnectionConfigurationValues.QueryStringParameters
        {
            get { return _queryString; }
        }

        IOrientdbSerializer IConnectionConfigurationValues.Serializer { get; set; }

        private BasicAuthenticationCredentials _basicAuthCredentials;

        BasicAuthenticationCredentials IConnectionConfigurationValues.BasicAuthorizationCredentials
        {
            get { return _basicAuthCredentials; }
        }

        public ConnectionConfiguration(IConnectionPool connectionPool)
        {
            _timeout = 60*1000;
            _connectionStatusHandler = ConnectionStatusDefaultHandler;
            _maximumAsyncConnections = 0;
            _connectionPool = connectionPool;
        }

        public ConnectionConfiguration(Uri uri = null)
            : this(new SingleNodeConnectionPool(uri ?? new Uri("http://localhost:2480")))
        {
        }

        public T MaximumRetries(int maxRetries)
        {
            _maxRetries = maxRetries;
            return (T) this;
        }

        public T SniffOnConnectionFault(bool sniffsOnConnectionFault = true)
        {
            _sniffOnConectionFault = sniffsOnConnectionFault;
            return (T) this;
        }

        public T SniffOnStartup(bool sniffsOnStartup = true)
        {
            _sniffOnStartup = sniffsOnStartup;
            return (T) this;
        }

        public T SniffLifeSpan(TimeSpan sniffTimeSpan)
        {
            _sniffLifeSpan = sniffTimeSpan;
            return (T) this;
        }

        public T EnableCompressedResponses(bool enabled = true)
        {
            _compressionEnabled = enabled;
            return (T) this;
        }

        /// <summary>
        ///     Enable Trace signals to the IConnection that it should put debug information on the Trace.
        /// </summary>
        public T EnableTrace(bool enabled = true)
        {
            _traceEnabled = enabled;
            return (T) this;
        }

        public T DisableAutomaticProxyDetection(bool disable = true)
        {
            _disableAutomaticProxyDetection = disable;
            return (T) this;
        }

        /// <summary>
        ///     By enabling metrics more metadata is returned per API call about requests (ping, sniff, failover) and general stats
        /// </summary>
        public T EnableMetrics(bool enabled = true)
        {
            _enableMetrics = enabled;
            return (T) this;
        }

        /// <summary>
        ///     Instead of following a c/go like error checking on response.IsValid always throw an ElasticsearchServerException
        ///     on the client when a call resulted in an exception on the elasticsearch server.
        ///     <para>Reasons for such exceptions could be search parser errors, index missing exceptions</para>
        /// </summary>
        public T ThrowOnOrientdbServerExceptions(bool alwaysThrow = true)
        {
            _throwOnServerExceptions = alwaysThrow;
            return (T) this;
        }

        /// <summary>
        ///     When a node is used for the very first time or when it's used for the first time after it has been marked dead
        ///     a ping with a very low timeout is send to the node to make sure that when it's still dead it reports it as fast as
        ///     possible.
        ///     You can disable these pings globally here if you rather have it fail on the possible slower original request
        /// </summary>
        public T DisablePing(bool disable = true)
        {
            _disablePings = disable;
            return (T) this;
        }

        /// <summary>
        ///     This NameValueCollection will be appended to every url NEST calls, great if you need to pass i.e an API key.
        /// </summary>
        public T SetGlobalQueryStringParameters(NameValueCollection queryStringParameters)
        {
            if (_queryString != null)
            {
                _queryString.Add(queryStringParameters);
            }
            _queryString = queryStringParameters;
            return (T) this;
        }

        /// <summary>
        ///     Timeout in milliseconds when the .NET webrequest should abort the request, note that you can set this to a high
        ///     value here,
        ///     and specify the timeout in various calls on Elasticsearch's side.
        /// </summary>
        /// <param name="timeout">time out in milliseconds</param>
        public T SetTimeout(int timeout)
        {
            _timeout = timeout;
            return (T) this;
        }

        /// <summary>
        ///     This is a separate timeout for Ping() requests. A ping should fail as fast as possible.
        /// </summary>
        /// <param name="timeout">The ping timeout in milliseconds defaults to 200</param>
        public T SetPingTimeout(int timeout)
        {
            _pingTimeout = timeout;
            return (T) this;
        }

        /// <summary>
        ///     Sets the default dead timeout factor when a node has been marked dead.
        /// </summary>
        /// <remarks>Some connection pools may use a flat timeout whilst others take this factor and increase it exponentially</remarks>
        /// <param name="timeout"></param>
        public T SetDeadTimeout(int timeout)
        {
            _deadTimeout = timeout;
            return (T) this;
        }

        /// <summary>
        ///     Sets the maximum time a node can be marked dead.
        ///     Different implementations of IConnectionPool may choose a different default.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds</param>
        public T SetMaxDeadTimeout(int timeout)
        {
            _maxDeadTimeout = timeout;
            return (T) this;
        }

        /// <summary>
        ///     Limits the total runtime including retries separately from <see cref="Timeout" />
        ///     <pre>
        ///         When not specified defaults to <see cref="Timeout" /> which itself defaults to 60seconds
        ///     </pre>
        /// </summary>
        public T SetMaxRetryTimeout(TimeSpan maxRetryTimeout)
        {
            _maxRetryTimeout = maxRetryTimeout;
            return (T) this;
        }

        /// <summary>
        ///     Semaphore asynchronous connections automatically by giving
        ///     it a maximum concurrent connections.
        /// </summary>
        /// <param name="maximum">defaults to 0 (unbounded)</param>
        public T SetMaximumAsyncConnections(int maximum)
        {
            _maximumAsyncConnections = maximum;
            return (T) this;
        }

        /// <summary>
        ///     If your connection has to go through proxy use this method to specify the proxy url
        /// </summary>
        public T SetProxy(Uri proxyAdress, string username, string password)
        {
            proxyAdress.ThrowIfNull("proxyAdress");
            _proxyAddress = proxyAdress.ToString();
            _proxyUsername = username;
            _proxyPassword = password;
            return (T) this;
        }

        /// <summary>
        ///     Append ?pretty=true to requests, this helps to debug send and received json.
        /// </summary>
        public T UsePrettyResponses(bool b = true)
        {
            _usePrettyResponses = b;
            SetGlobalQueryStringParameters(new NameValueCollection {{"pretty", b.ToString().ToLowerInvariant()}});
            return (T) this;
        }

        /// <summary>
        ///     Make sure the reponse bytes are always available on the ElasticsearchResponse object
        ///     <para>Note: that depending on the registered serializer this may cause the respond to be read in memory first</para>
        /// </summary>
        public T ExposeRawResponse(bool b = true)
        {
            _keepRawResponse = b;
            return (T) this;
        }

        protected void ConnectionStatusDefaultHandler(IOrientdbResponse status)
        {
        }

        /// <summary>
        ///     Global callback for every response that NEST receives, useful for custom logging.
        /// </summary>
        public T SetConnectionStatusHandler(Action<IOrientdbResponse> handler)
        {
            handler.ThrowIfNull("handler");
            _connectionStatusHandler = handler;
            return (T) this;
        }

        ///// <summary>
        ///// Basic access authentication credentials to specify with all requests.
        ///// </summary>
        //[Obsolete("Scheduled to be removed in 2.0.  Use SetBasicAuthentication() instead.")]
        //public T SetBasicAuthorization(string userName, string password)
        //{
        //    return this.SetBasicAuthentication(userName, password);
        //}

        /// <summary>
        ///     Basic access authentication credentials to specify with all requests.
        /// </summary>
        public T SetBasicAuthentication(string userName, string password)
        {
            if (_basicAuthCredentials == null)
                _basicAuthCredentials = new BasicAuthenticationCredentials();
            _basicAuthCredentials.UserName = userName;
            _basicAuthCredentials.Password = password;
            return (T) this;
        }

        /// <summary>
        ///     Allows for requests to be pipelined. http://en.wikipedia.org/wiki/HTTP_pipelining
        ///     <para>Note: HTTP pipelining must also be enabled in Elasticsearch for this to work properly.</para>
        /// </summary>
        public T HttpPipeliningEnabled(bool enabled = true)
        {
            _httpPipeliningEnabled = enabled;
            return (T) this;
        }
    }
}