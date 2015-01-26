using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public enum NodesStatsMetric
    {
        [EnumMember(Value = "_all")]
        All,
        [EnumMember(Value = "breaker")]
        Breaker,
        [EnumMember(Value = "fs")]
        Fs,
        [EnumMember(Value = "http")]
        Http,
        [EnumMember(Value = "indices")]
        Indices,
        [EnumMember(Value = "jvm")]
        Jvm,
        [EnumMember(Value = "network")]
        Network,
        [EnumMember(Value = "os")]
        Os,
        [EnumMember(Value = "process")]
        Process,
        [EnumMember(Value = "thread_pool")]
        ThreadPool,
        [EnumMember(Value = "transport")]
        Transport
    }
}