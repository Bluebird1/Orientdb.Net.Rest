using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Orientdb.Net.Connection.RequestState;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.Connection
// ReSharper restore CheckNamespace
{
    internal interface ITransportDelegator
    {
        bool Ping(ITransportRequestState requestState);
        Task<bool> PingAsync(ITransportRequestState requestState);
        IList<Uri> Sniff(ITransportRequestState ownerState = null);
        void SniffClusterState(ITransportRequestState requestState = null);
        void SniffOnStaleClusterState(ITransportRequestState requestState);
        void SniffOnConnectionFailure(ITransportRequestState requestState);

        int GetMaximumRetries(IRequestConfiguration requestConfiguration);

        bool SniffingDisabled(IRequestConfiguration requestConfiguration);

        bool SniffOnFaultDiscoveredMoreNodes(ITransportRequestState requestState, int retried,
            OrientdbResponse<Stream> streamResponse);

        bool TookTooLongToRetry(ITransportRequestState requestState);

        bool SelectNextNode(ITransportRequestState requestState);
    }
}