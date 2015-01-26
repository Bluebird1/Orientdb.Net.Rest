using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Orientdb.Net.Connection.RequestState
{
    internal class RequestTimings : IRequestTimings
    {
        private readonly Uri _node;
        private readonly string _path;
        private readonly List<RequestMetrics> _requestMetrics;
        private readonly DateTime _startedOn;
        private readonly Stopwatch _stopwatch;
        private readonly RequestType _type;
        private int? _httpStatusCode;
        private bool _success;

        public RequestTimings(RequestType type, Uri node, string path, List<RequestMetrics> requestMetrics)
        {
            _startedOn = DateTime.UtcNow;
            _node = node;
            _path = path;
            _requestMetrics = requestMetrics;
            _type = type;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Finish(bool success, int? httpStatusCode)
        {
            _stopwatch.Stop();
            _success = success;
            _httpStatusCode = httpStatusCode;
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _requestMetrics.Add(new RequestMetrics
            {
                StartedOn = _startedOn,
                Node = _node,
                EllapsedMilliseconds = _stopwatch.ElapsedMilliseconds,
                Path = _path,
                RequestType = _type,
                Success = _success,
                HttpStatusCode = _httpStatusCode
            });
        }
    }
}