using System;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public static class KnownEnums
    {
        public static string Resolve(Enum e)
        {

            if (e is Consistency)
            {
                switch ((Consistency)e)
                {
                    case Consistency.One: return "one";
                    case Consistency.Quorum: return "quorum";
                    case Consistency.All: return "all";
                }
            }


            if (e is Replication)
            {
                switch ((Replication)e)
                {
                    case Replication.Sync: return "sync";
                    case Replication.Async: return "async";
                }
            }


            if (e is Bytes)
            {
                switch ((Bytes)e)
                {
                    case Bytes.B: return "b";
                    case Bytes.K: return "k";
                    case Bytes.M: return "m";
                    case Bytes.G: return "g";
                }
            }


            if (e is Level)
            {
                switch ((Level)e)
                {
                    case Level.Cluster: return "cluster";
                    case Level.Indices: return "indices";
                    case Level.Shards: return "shards";
                }
            }


            if (e is WaitForStatus)
            {
                switch ((WaitForStatus)e)
                {
                    case WaitForStatus.Green: return "green";
                    case WaitForStatus.Yellow: return "yellow";
                    case WaitForStatus.Red: return "red";
                }
            }


            if (e is ExpandWildcards)
            {
                switch ((ExpandWildcards)e)
                {
                    case ExpandWildcards.Open: return "open";
                    case ExpandWildcards.Closed: return "closed";
                }
            }


            if (e is VersionType)
            {
                switch ((VersionType)e)
                {
                    case VersionType.Internal: return "internal";
                    case VersionType.External: return "external";
                    case VersionType.ExternalGte: return "external_gte";
                    case VersionType.Force: return "force";
                }
            }


            if (e is DefaultOperator)
            {
                switch ((DefaultOperator)e)
                {
                    case DefaultOperator.And: return "AND";
                    case DefaultOperator.Or: return "OR";
                }
            }


            if (e is OpType)
            {
                switch ((OpType)e)
                {
                    case OpType.Index: return "index";
                    case OpType.Create: return "create";
                }
            }


            if (e is Format)
            {
                switch ((Format)e)
                {
                    case Format.Detailed: return "detailed";
                    case Format.Text: return "text";
                }
            }


            if (e is SearchType)
            {
                switch ((SearchType)e)
                {
                    case SearchType.QueryThenFetch: return "query_then_fetch";
                    case SearchType.QueryAndFetch: return "query_and_fetch";
                    case SearchType.DfsQueryThenFetch: return "dfs_query_then_fetch";
                    case SearchType.DfsQueryAndFetch: return "dfs_query_and_fetch";
                    case SearchType.Count: return "count";
                    case SearchType.Scan: return "scan";
                }
            }


            if (e is ThreadType)
            {
                switch ((ThreadType)e)
                {
                    case ThreadType.Cpu: return "cpu";
                    case ThreadType.Wait: return "wait";
                    case ThreadType.Block: return "block";
                }
            }


            if (e is PercolateFormat)
            {
                switch ((PercolateFormat)e)
                {
                    case PercolateFormat.Ids: return "ids";
                }
            }


            if (e is SuggestMode)
            {
                switch ((SuggestMode)e)
                {
                    case SuggestMode.Missing: return "missing";
                    case SuggestMode.Popular: return "popular";
                    case SuggestMode.Always: return "always";
                }
            }


            if (e is ClusterStateMetric)
            {
                switch ((ClusterStateMetric)e)
                {
                    case ClusterStateMetric.All: return "_all";
                    case ClusterStateMetric.Blocks: return "blocks";
                    case ClusterStateMetric.Metadata: return "metadata";
                    case ClusterStateMetric.Nodes: return "nodes";
                    case ClusterStateMetric.RoutingTable: return "routing_table";
                    case ClusterStateMetric.MasterNode: return "master_node";
                    case ClusterStateMetric.Version: return "version";
                }
            }


            if (e is IndicesStatsMetric)
            {
                switch ((IndicesStatsMetric)e)
                {
                    case IndicesStatsMetric.All: return "_all";
                    case IndicesStatsMetric.Completion: return "completion";
                    case IndicesStatsMetric.Docs: return "docs";
                    case IndicesStatsMetric.Fielddata: return "fielddata";
                    case IndicesStatsMetric.FilterCache: return "filter_cache";
                    case IndicesStatsMetric.Flush: return "flush";
                    case IndicesStatsMetric.Get: return "get";
                    case IndicesStatsMetric.IdCache: return "id_cache";
                    case IndicesStatsMetric.Indexing: return "indexing";
                    case IndicesStatsMetric.Merge: return "merge";
                    case IndicesStatsMetric.Percolate: return "percolate";
                    case IndicesStatsMetric.QueryCache: return "query_cache";
                    case IndicesStatsMetric.Refresh: return "refresh";
                    case IndicesStatsMetric.Search: return "search";
                    case IndicesStatsMetric.Segments: return "segments";
                    case IndicesStatsMetric.Store: return "store";
                    case IndicesStatsMetric.Warmer: return "warmer";
                    case IndicesStatsMetric.Suggest: return "suggest";
                }
            }


            if (e is NodesInfoMetric)
            {
                switch ((NodesInfoMetric)e)
                {
                    case NodesInfoMetric.Settings: return "settings";
                    case NodesInfoMetric.Os: return "os";
                    case NodesInfoMetric.Process: return "process";
                    case NodesInfoMetric.Jvm: return "jvm";
                    case NodesInfoMetric.ThreadPool: return "thread_pool";
                    case NodesInfoMetric.Network: return "network";
                    case NodesInfoMetric.Transport: return "transport";
                    case NodesInfoMetric.Http: return "http";
                    case NodesInfoMetric.Plugins: return "plugins";
                }
            }


            if (e is NodesStatsMetric)
            {
                switch ((NodesStatsMetric)e)
                {
                    case NodesStatsMetric.All: return "_all";
                    case NodesStatsMetric.Breaker: return "breaker";
                    case NodesStatsMetric.Fs: return "fs";
                    case NodesStatsMetric.Http: return "http";
                    case NodesStatsMetric.Indices: return "indices";
                    case NodesStatsMetric.Jvm: return "jvm";
                    case NodesStatsMetric.Network: return "network";
                    case NodesStatsMetric.Os: return "os";
                    case NodesStatsMetric.Process: return "process";
                    case NodesStatsMetric.ThreadPool: return "thread_pool";
                    case NodesStatsMetric.Transport: return "transport";
                }
            }


            if (e is NodesStatsIndexMetric)
            {
                switch ((NodesStatsIndexMetric)e)
                {
                    case NodesStatsIndexMetric.All: return "_all";
                    case NodesStatsIndexMetric.Completion: return "completion";
                    case NodesStatsIndexMetric.Docs: return "docs";
                    case NodesStatsIndexMetric.Fielddata: return "fielddata";
                    case NodesStatsIndexMetric.FilterCache: return "filter_cache";
                    case NodesStatsIndexMetric.Flush: return "flush";
                    case NodesStatsIndexMetric.Get: return "get";
                    case NodesStatsIndexMetric.IdCache: return "id_cache";
                    case NodesStatsIndexMetric.Indexing: return "indexing";
                    case NodesStatsIndexMetric.Merge: return "merge";
                    case NodesStatsIndexMetric.Percolate: return "percolate";
                    case NodesStatsIndexMetric.QueryCache: return "query_cache";
                    case NodesStatsIndexMetric.Refresh: return "refresh";
                    case NodesStatsIndexMetric.Search: return "search";
                    case NodesStatsIndexMetric.Segments: return "segments";
                    case NodesStatsIndexMetric.Store: return "store";
                    case NodesStatsIndexMetric.Warmer: return "warmer";
                    case NodesStatsIndexMetric.Suggest: return "suggest";
                }
            }

            return "UNKNOWNENUM";
        }
    }
}