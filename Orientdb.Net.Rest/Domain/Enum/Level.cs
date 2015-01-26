using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public enum Level
    {
        [EnumMember(Value = "cluster")]
        Cluster,
        [EnumMember(Value = "indices")]
        Indices,
        [EnumMember(Value = "shards")]
        Shards
    }
}