using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public enum NodesStatsIndexMetric
    {
        [EnumMember(Value = "_all")]
        All,
        [EnumMember(Value = "completion")]
        Completion,
        [EnumMember(Value = "docs")]
        Docs,
        [EnumMember(Value = "fielddata")]
        Fielddata,
        [EnumMember(Value = "filter_cache")]
        FilterCache,
        [EnumMember(Value = "flush")]
        Flush,
        [EnumMember(Value = "get")]
        Get,
        [EnumMember(Value = "id_cache")]
        IdCache,
        [EnumMember(Value = "indexing")]
        Indexing,
        [EnumMember(Value = "merge")]
        Merge,
        [EnumMember(Value = "percolate")]
        Percolate,
        [EnumMember(Value = "query_cache")]
        QueryCache,
        [EnumMember(Value = "refresh")]
        Refresh,
        [EnumMember(Value = "search")]
        Search,
        [EnumMember(Value = "segments")]
        Segments,
        [EnumMember(Value = "store")]
        Store,
        [EnumMember(Value = "warmer")]
        Warmer,
        [EnumMember(Value = "suggest")]
        Suggest
    }
}