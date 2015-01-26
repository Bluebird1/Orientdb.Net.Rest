using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public enum SearchType
    {
        [EnumMember(Value = "query_then_fetch")]
        QueryThenFetch,
        [EnumMember(Value = "query_and_fetch")]
        QueryAndFetch,
        [EnumMember(Value = "dfs_query_then_fetch")]
        DfsQueryThenFetch,
        [EnumMember(Value = "dfs_query_and_fetch")]
        DfsQueryAndFetch,
        [EnumMember(Value = "count")]
        Count,
        [EnumMember(Value = "scan")]
        Scan
    }
}