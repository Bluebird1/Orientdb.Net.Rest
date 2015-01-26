﻿using System;
using System.Collections.Generic;

namespace Orientdb.Net.ConnectionPool
{
    public class SingleNodeConnectionPool : IConnectionPool
    {
        private readonly Uri _uri;
        public int MaxRetries { get { return 0; } }

        public bool AcceptsUpdates { get { return false; } }

        public SingleNodeConnectionPool(Uri uri)
        {
            if (!uri.OriginalString.EndsWith("/"))
                uri = new Uri(uri.OriginalString + "/");
            _uri = uri;
        }

        public Uri GetNext(int? initialSeed, out int seed, out bool shouldPingHint)
        {
            seed = 0;
            shouldPingHint = false;
            return _uri;
        }

        public void MarkDead(Uri uri, int? deadTimeout = null, int? maxDeadTimeout = null)
        {
        }

        public void MarkAlive(Uri uri)
        {
        }

        public void UpdateNodeList(IList<Uri> newClusterState, Uri sniffNode = null)
        {
        }

    }
}