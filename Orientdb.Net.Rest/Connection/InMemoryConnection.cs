using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Orientdb.Net.Connection
{
    public class InMemoryConnection : HttpConnection
    {
        private readonly byte[] _fixedResultBytes =
            Encoding.UTF8.GetBytes("{ \"USING NEST IN MEMORY CONNECTION\"  : null }");

        private readonly int _statusCode;

        public List<Tuple<string, Uri, byte[]>> Requests = new List<Tuple<string, Uri, byte[]>>();

        public InMemoryConnection()
            : base(new ConnectionConfiguration())
        {
        }

        public InMemoryConnection(IConnectionConfigurationValues settings)
            : base(settings)
        {
            _statusCode = 200;
        }

        public InMemoryConnection(IConnectionConfigurationValues settings, string fixedResult, int statusCode = 200)
            : this(settings)
        {
            _fixedResultBytes = Encoding.UTF8.GetBytes(fixedResult);
            _statusCode = statusCode;
        }

        public bool RecordRequests { get; set; }


        protected override OrientdbResponse<Stream> DoSynchronousRequest(HttpWebRequest request, byte[] data = null,
            IRequestConfiguration requestSpecificConfig = null)
        {
            return ReturnConnectionStatus(request, data, requestSpecificConfig);
        }

        private OrientdbResponse<Stream> ReturnConnectionStatus(HttpWebRequest request, byte[] data,
            IRequestConfiguration requestSpecificConfig = null)
        {
            string method = request.Method;
            string path = request.RequestUri.ToString();

            OrientdbResponse<Stream> cs = OrientdbResponse<Stream>.Create(ConnectionSettings, _statusCode, method, path,
                data);
            cs.Response = new MemoryStream(_fixedResultBytes);
            if (ConnectionSettings.ConnectionStatusHandler != null)
                ConnectionSettings.ConnectionStatusHandler(cs);

            if (RecordRequests)
            {
                Requests.Add(Tuple.Create(method, request.RequestUri, data));
            }

            return cs;
        }

        protected override Task<OrientdbResponse<Stream>> DoAsyncRequest(HttpWebRequest request, byte[] data = null,
            IRequestConfiguration requestSpecificConfig = null)
        {
            return Task.Factory.StartNew(() =>
            {
                OrientdbResponse<Stream> cs = ReturnConnectionStatus(request, data, requestSpecificConfig);
                return cs;
            });
        }
    }
}