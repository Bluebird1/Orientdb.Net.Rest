using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public enum NodesInfoMetric
    {
        [EnumMember(Value = "settings")]
        Settings,
        [EnumMember(Value = "os")]
        Os,
        [EnumMember(Value = "process")]
        Process,
        [EnumMember(Value = "jvm")]
        Jvm,
        [EnumMember(Value = "thread_pool")]
        ThreadPool,
        [EnumMember(Value = "network")]
        Network,
        [EnumMember(Value = "transport")]
        Transport,
        [EnumMember(Value = "http")]
        Http,
        [EnumMember(Value = "plugins")]
        Plugins
    }
}