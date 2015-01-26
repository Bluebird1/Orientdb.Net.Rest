using System;
using System.Collections.Generic;
using System.IO;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.Connection.RequestState
// ReSharper restore CheckNamespace
{
    public interface ITransportRequestState
    {
        IRequestConfiguration RequestConfiguration { get; }
        int Retried { get; }
        DateTime StartedOn { get; }
        bool SniffedOnConnectionFailure { get; set; }
        int? Seed { get; set; }
        Uri CurrentNode { get; set; }
        List<RequestMetrics> RequestMetrics { get; set; }
        List<Exception> SeenExceptions { get; }
        Func<IOrientdbResponse, Stream, object> ResponseCreationOverride { get; set; }
        bool UsingPooling { get; }
        IRequestTimings InitiateRequest(RequestType requestType);
        Uri CreatePathOnCurrentNode(string path);
    }
}