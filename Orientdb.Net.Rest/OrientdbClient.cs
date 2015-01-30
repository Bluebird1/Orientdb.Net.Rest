using System;
using System.Threading.Tasks;
using Orientdb.Net.Connection;
using Orientdb.Net.Serialization;

namespace Orientdb.Net
{
    public partial class OrientdbClient : IOrientdbClient
    {
        public OrientdbClient(
            IConnectionConfigurationValues settings = null,
            IConnection connection = null,
            ITransport transport = null,
            IOrientdbSerializer serializer = null
            )
        {
            settings = settings ?? new ConnectionConfiguration();
            Transport = transport ?? new Transport(settings, connection, serializer);
            Settings.Serializer = Transport.Serializer;
        }

        public IConnectionConfigurationValues Settings
        {
            get { return Transport.Settings; }
        }

        public IOrientdbSerializer Serializer
        {
            get { return Transport.Serializer; }
        }

        protected ITransport Transport { get; set; }

        /// <summary>
        ///     Perform any request you want over the configured IConnection synchronously while taking advantage of the cluster
        ///     failover.
        /// </summary>
        /// <typeparam name="T">The type representing the response JSON</typeparam>
        /// <param name="method">the HTTP Method to use</param>
        /// <param name="path">The path of the the url that you would like to hit</param>
        /// <param name="data">The body of the request, string and byte[] are posted as is other types will be serialized to JSON</param>
        /// <param name="requestParameters">Optionally configure request specific timeouts, headers</param>
        /// <returns>An OrientdbResponse of T where T represents the JSON response body</returns>
        public OrientdbResponse<T> DoRequest<T>(string method, string path, object data = null,
            IRequestParameters requestParameters = null)
        {
            return Transport.DoRequest<T>(method, path, data, requestParameters);
        }

        /// <summary>
        ///     Perform any request you want over the configured IConnection asynchronously while taking advantage of the cluster
        ///     failover.
        /// </summary>
        /// <typeparam name="T">The type representing the response JSON</typeparam>
        /// <param name="method">the HTTP Method to use</param>
        /// <param name="path">The path of the the url that you would like to hit</param>
        /// <param name="data">The body of the request, string and byte[] are posted as is other types will be serialized to JSON</param>
        /// <param name="requestParameters">Optionally configure request specific timeouts, headers</param>
        /// <returns>A task of OrientdbResponse of T where T represents the JSON response body</returns>
        public Task<OrientdbResponse<T>> DoRequestAsync<T>(string method, string path, object data = null,
            IRequestParameters requestParameters = null)
        {
            return Transport.DoRequestAsync<T>(method, path, data, requestParameters);
        }

        public string Encoded(object o)
        {
            return Uri.EscapeDataString(Serializer.Stringify(o));
        }
    }
}