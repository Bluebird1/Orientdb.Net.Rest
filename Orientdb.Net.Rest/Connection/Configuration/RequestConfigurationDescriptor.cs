using System;
using System.Collections.Generic;
using Orientdb.Net.Connection.Security;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.Connection
// ReSharper restore CheckNamespace
{
    public class RequestConfigurationDescriptor : IRequestConfiguration
    {
        private IRequestConfiguration Self { get { return this; } }

        int? IRequestConfiguration.RequestTimeout { get; set; }

        int? IRequestConfiguration.ConnectTimeout { get; set; }

        string IRequestConfiguration.ContentType { get; set; }

        int? IRequestConfiguration.MaxRetries { get; set; }

        Uri IRequestConfiguration.ForceNode { get; set; }

        bool? IRequestConfiguration.DisableSniff { get; set; }

        bool? IRequestConfiguration.DisablePing { get; set; }

        IEnumerable<int> IRequestConfiguration.AllowedStatusCodes { get; set; }

        public BasicAuthenticationCredentials BasicAuthenticationCredentials { get; set; }
    

        bool IRequestConfiguration.EnableHttpPipelining { get; set; }

        public RequestConfigurationDescriptor RequestTimeout(int requestTimeoutInMilliseconds)
        {
            Self.RequestTimeout = requestTimeoutInMilliseconds;
            return this;
        }

        public RequestConfigurationDescriptor ConnectTimeout(int connectTimeoutInMilliseconds)
        {
            Self.ConnectTimeout = connectTimeoutInMilliseconds;
            return this;
        }

        public RequestConfigurationDescriptor AcceptContentType(string acceptContentTypeHeader)
        {
            Self.ContentType = acceptContentTypeHeader;
            return this;
        }

        public RequestConfigurationDescriptor AllowedStatusCodes(IEnumerable<int> codes)
        {
            Self.AllowedStatusCodes = codes;
            return this;
        }

        public RequestConfigurationDescriptor AllowedStatusCodes(params int[] codes)
        {
            Self.AllowedStatusCodes = codes;
            return this;
        }

        public RequestConfigurationDescriptor DisableSniffing(bool? disable = true)
        {
            Self.DisableSniff = disable;
            return this;
        }

        public RequestConfigurationDescriptor DisablePing(bool? disable = true)
        {
            Self.DisablePing = disable;
            return this;
        }

        public RequestConfigurationDescriptor ForceNode(Uri uri)
        {
            Self.ForceNode = uri;
            return this;
        }
        public RequestConfigurationDescriptor MaxRetries(int retry)
        {
            Self.MaxRetries = retry;
            return this;
        }

        [Obsolete("Scheduled to be removed in 2.0.  Use BasicAuthentication() instead.")]
        public RequestConfigurationDescriptor BasicAuthorization(string userName, string password)
        {
            return this.BasicAuthentication(userName, password);
        }

        public RequestConfigurationDescriptor BasicAuthentication(string userName, string password)
        {
            if (Self.BasicAuthenticationCredentials == null)
                Self.BasicAuthenticationCredentials = new BasicAuthenticationCredentials();
            Self.BasicAuthenticationCredentials.UserName = userName;
            Self.BasicAuthenticationCredentials.Password = password;
            return this;
        }

        public RequestConfigurationDescriptor EnableHttpPipelining(bool enable = true)
        {
            Self.EnableHttpPipelining = enable;
            return this;
        }
    }
}