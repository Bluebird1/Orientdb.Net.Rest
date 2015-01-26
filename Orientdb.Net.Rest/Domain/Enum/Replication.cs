using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public enum Replication
    {
        [EnumMember(Value = "sync")]
        Sync,
        [EnumMember(Value = "async")]
        Async
    }
}