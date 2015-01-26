﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Orientdb.Net.Connection.RequestHandlers;
using Orientdb.Net.Connection.RequestState;
using Orientdb.Net.ConnectionPool;
using Orientdb.Net.Exceptions;
using Orientdb.Net.Providers;
using Orientdb.Net.Serialization;

// ReSharper disable CheckNamespace

namespace Orientdb.Net.Connection
// ReSharper restore CheckNamespace
{
    public class Transport : ITransport, ITransportDelegator
    {
        private const int DefaultPingTimeout = 200;
        protected internal readonly IConnectionConfigurationValues ConfigurationValues;
        protected internal readonly IConnection Connection;

        private readonly IConnectionPool _connectionPool;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IMemoryStreamProvider _memoryStreamProvider;
        private readonly RequestHandler _requestHandler;
        private readonly RequestHandlerAsync _requestHandlerAsync;
        private readonly IOrientdbSerializer _serializer;

        private DateTime? _lastSniff;

        public Transport(
            IConnectionConfigurationValues configurationValues,
            IConnection connection,
            IOrientdbSerializer serializer,
            IDateTimeProvider dateTimeProvider = null,
            IMemoryStreamProvider memoryStreamProvider = null
            )
        {
            ConfigurationValues = configurationValues;
            Connection = connection ?? new HttpConnection(configurationValues);
            //_serializer = serializer ?? new OrientdbDefaultSerializer();
            _serializer = serializer ?? new OrientdbSerializer();

            _connectionPool = ConfigurationValues.ConnectionPool;

            _dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();
            _memoryStreamProvider = memoryStreamProvider ?? new MemoryStreamProvider();

            _lastSniff = _dateTimeProvider.Now();

            Settings.Serializer = _serializer;

            _requestHandler = new RequestHandler(Settings, _connectionPool, Connection, _serializer,
                _memoryStreamProvider, this);
            _requestHandlerAsync = new RequestHandlerAsync(Settings, _connectionPool, Connection, _serializer,
                _memoryStreamProvider, this);
            if (_connectionPool.AcceptsUpdates && Settings.SniffsOnStartup)
                Self.SniffClusterState();
        }

        private ITransportDelegator Self
        {
            get { return this; }
        }

        public IConnectionConfigurationValues Settings
        {
            get { return ConfigurationValues; }
        }

        public IOrientdbSerializer Serializer
        {
            get { return _serializer; }
        }

        public OrientdbResponse<T> DoRequest<T>(string method, string path, object data = null,
            IRequestParameters requestParameters = null)
        {
            using (var requestState = new TransportRequestState<T>(Settings, requestParameters, method, path))
            {
                return _requestHandler.Request(requestState, data);
            }
        }

        public Task<OrientdbResponse<T>> DoRequestAsync<T>(string method, string path, object data = null,
            IRequestParameters requestParameters = null)
        {
            using (var requestState = new TransportRequestState<T>(Settings, requestParameters, method, path))
            {
                return _requestHandlerAsync.RequestAsync(requestState, data);
            }
        }


        /* PING/SNIFF	*** ********************************************/

        bool ITransportDelegator.Ping(ITransportRequestState requestState)
        {
            int pingTimeout = Settings.PingTimeout.GetValueOrDefault(DefaultPingTimeout);
            pingTimeout = requestState.RequestConfiguration != null
                ? requestState.RequestConfiguration.ConnectTimeout.GetValueOrDefault(pingTimeout)
                : pingTimeout;
            var requestOverrides = new RequestConfiguration
            {
                ConnectTimeout = pingTimeout,
                RequestTimeout = pingTimeout
            };
            try
            {
                OrientdbResponse<Stream> response;
                using (IRequestTimings rq = requestState.InitiateRequest(RequestType.Ping))
                {
                    response = Connection.HeadSync(requestState.CreatePathOnCurrentNode(""), requestOverrides);
                    rq.Finish(response.Success, response.HttpStatusCode);
                }
                if (!response.HttpStatusCode.HasValue || response.HttpStatusCode.Value == -1)
                    throw new Exception("ping returned no status code", response.OriginalException);
                if (response.HttpStatusCode == (int) HttpStatusCode.Unauthorized)
                    throw new OrientdbAuthenticationException(response);
                if (response.Response == null)
                    return response.Success;
                using (response.Response)
                    return response.Success;
            }
            catch (OrientdbAuthenticationException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new PingException(requestState.CurrentNode, e);
            }
        }

        Task<bool> ITransportDelegator.PingAsync(ITransportRequestState requestState)
        {
            int pingTimeout = Settings.PingTimeout.GetValueOrDefault(DefaultPingTimeout);
            pingTimeout = requestState.RequestConfiguration != null
                ? requestState.RequestConfiguration.ConnectTimeout.GetValueOrDefault(pingTimeout)
                : pingTimeout;
            var requestOverrides = new RequestConfiguration
            {
                ConnectTimeout = pingTimeout,
                RequestTimeout = pingTimeout
            };
            IRequestTimings rq = requestState.InitiateRequest(RequestType.Ping);
            try
            {
                return Connection.Head(requestState.CreatePathOnCurrentNode(""), requestOverrides)
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            rq.Finish(false, null);
                            rq.Dispose();
                            throw new PingException(requestState.CurrentNode, t.Exception);
                        }
                        rq.Finish(t.Result.Success, t.Result.HttpStatusCode);
                        rq.Dispose();
                        OrientdbResponse<Stream> response = t.Result;
                        if (!response.HttpStatusCode.HasValue || response.HttpStatusCode.Value == -1)
                            throw new PingException(requestState.CurrentNode, t.Exception);
                        if (response.HttpStatusCode == (int) HttpStatusCode.Unauthorized)
                            throw new OrientdbAuthenticationException(response);
                        using (response.Response)
                            return response.Success;
                    });
            }
            catch (OrientdbAuthenticationException)
            {
                throw;
            }
            catch (Exception e)
            {
                var tcs = new TaskCompletionSource<bool>();
                var pingException = new PingException(requestState.CurrentNode, e);
                tcs.SetException(pingException);
                return tcs.Task;
            }
        }

        IList<Uri> ITransportDelegator.Sniff(ITransportRequestState ownerState = null)
        {
            int pingTimeout = Settings.PingTimeout.GetValueOrDefault(DefaultPingTimeout);
            var requestOverrides = new RequestConfiguration
            {
                ConnectTimeout = pingTimeout,
                RequestTimeout = pingTimeout,
                DisableSniff = true //sniff call should never recurse 
            };

            var requestParameters = new RequestParameters {RequestConfiguration = requestOverrides};
            try
            {
                string path = "_nodes/_all/clear?timeout=" + pingTimeout;
                OrientdbResponse<Stream> response;
                using (var requestState = new TransportRequestState<Stream>(Settings, requestParameters, "GET", path))
                {
                    response = _requestHandler.Request(requestState);

                    //inform the owing request state of the requests the sniffs did.
                    if (requestState.RequestMetrics != null && ownerState != null)
                    {
                        foreach (
                            RequestMetrics r in
                                requestState.RequestMetrics.Where(p => p.RequestType == RequestType.OrientdbCall))
                            r.RequestType = RequestType.Sniff;


                        if (ownerState.RequestMetrics == null) ownerState.RequestMetrics = new List<RequestMetrics>();
                        ownerState.RequestMetrics.AddRange(requestState.RequestMetrics);
                    }
                    if (response.HttpStatusCode.HasValue && response.HttpStatusCode == (int) HttpStatusCode.Unauthorized)
                        throw new OrientdbAuthenticationException(response);
                    if (response.Response == null)
                        return null;

                    using (response.Response)
                    {
                        return Sniffer.FromStream(response, response.Response, Serializer);
                    }
                }
            }
            catch (MaxRetryException e)
            {
                throw new MaxRetryException(new SniffException(e));
            }
        }

        void ITransportDelegator.SniffClusterState(ITransportRequestState requestState = null)
        {
            if (!_connectionPool.AcceptsUpdates)
                return;

            IList<Uri> newClusterState = Self.Sniff(requestState);
            if (!newClusterState.HasAny())
                return;

            _connectionPool.UpdateNodeList(newClusterState);
            _lastSniff = _dateTimeProvider.Now();
        }

        void ITransportDelegator.SniffOnStaleClusterState(ITransportRequestState requestState)
        {
            if (Self.SniffingDisabled(requestState.RequestConfiguration))
                return;

            TimeSpan? sniffLifeSpan = ConfigurationValues.SniffInformationLifeSpan;
            DateTime now = _dateTimeProvider.Now();
            if (requestState.Retried == 0 && _lastSniff.HasValue &&
                sniffLifeSpan.HasValue && sniffLifeSpan.Value < (now - _lastSniff.Value))
                Self.SniffClusterState(requestState);
        }

        void ITransportDelegator.SniffOnConnectionFailure(ITransportRequestState requestState)
        {
            if (requestState.SniffedOnConnectionFailure
                || !requestState.UsingPooling
                || Self.SniffingDisabled(requestState.RequestConfiguration)
                || !ConfigurationValues.SniffsOnConnectionFault
                || requestState.Retried != 0) return;

            Self.SniffClusterState(requestState);
            requestState.SniffedOnConnectionFailure = true;
        }

        /* REQUEST STATE *** ********************************************/

        /// <summary>
        ///     Returns whether the current delegation over nodes took too long and we should quit.
        ///     if <see cref="ConnectionSettings.SetMaxRetryTimeout" /> is set we'll use that timeout otherwise we default to th
        ///     value of
        ///     <see cref="ConnectionSettings.SetTimeout" /> which itself defaults to 60 seconds
        /// </summary>
        bool ITransportDelegator.TookTooLongToRetry(ITransportRequestState requestState)
        {
            TimeSpan timeout = Settings.MaxRetryTimeout.GetValueOrDefault(TimeSpan.FromMilliseconds(Settings.Timeout));
            DateTime startedOn = requestState.StartedOn;
            DateTime now = _dateTimeProvider.Now();

            //we apply a soft margin so that if a request timesout at 59 seconds when the maximum is 60
            //we also abort.
            double margin = (timeout.TotalMilliseconds/100.0)*98;
            TimeSpan marginTimeSpan = TimeSpan.FromMilliseconds(margin);
            TimeSpan timespanCall = (now - startedOn);
            bool tookToLong = timespanCall >= marginTimeSpan;
            return tookToLong;
        }

        /// <summary>
        ///     Returns either the fixed maximum set on the connection configuration settings or the number of nodes
        /// </summary>
        int ITransportDelegator.GetMaximumRetries(IRequestConfiguration requestConfiguration)
        {
            //if we have a request specific max retry setting use that
            if (requestConfiguration != null && requestConfiguration.MaxRetries.HasValue)
                return requestConfiguration.MaxRetries.Value;

            return ConfigurationValues.MaxRetries.GetValueOrDefault(_connectionPool.MaxRetries);
        }

        bool ITransportDelegator.SniffingDisabled(IRequestConfiguration requestConfiguration)
        {
            if (!_connectionPool.AcceptsUpdates)
                return true;
            if (requestConfiguration == null)
                return false;
            return requestConfiguration.DisableSniff.GetValueOrDefault(false);
        }

        bool ITransportDelegator.SniffOnFaultDiscoveredMoreNodes(ITransportRequestState requestState, int retried,
            OrientdbResponse<Stream> streamResponse)
        {
            if (!requestState.UsingPooling || retried != 0 ||
                (streamResponse != null && streamResponse.SuccessOrKnownError)) return false;
            Self.SniffOnConnectionFailure(requestState);
            return Self.GetMaximumRetries(requestState.RequestConfiguration) > 0;
        }

        /// <summary>
        ///     Selects next node uri on request state
        /// </summary>
        /// <returns>bool hint whether the new current node needs to pinged first</returns>
        bool ITransportDelegator.SelectNextNode(ITransportRequestState requestState)
        {
            if (requestState.RequestConfiguration != null && requestState.RequestConfiguration.ForceNode != null)
            {
                requestState.Seed = 0;
                return false;
            }
            int initialSeed;
            bool shouldPingHint;
            Uri baseUri = _connectionPool.GetNext(requestState.Seed, out initialSeed, out shouldPingHint);
            requestState.Seed = initialSeed;
            requestState.CurrentNode = baseUri;
            return shouldPingHint
                   && !ConfigurationValues.DisablePings
                   && (requestState.RequestConfiguration == null
                       || !requestState.RequestConfiguration.DisablePing.GetValueOrDefault(false));
        }
    }
}