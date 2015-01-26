using System;
using System.Collections.Generic;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public class CallMetrics
    {
        public string Path { get; set; }
        public long SerializationTime { get; set; }
        public long DeserializationTime { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime CompletedOn { get; set; }
        public List<RequestMetrics> Requests { get; set; }
    }
}