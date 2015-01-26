using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public enum OpType
    {
        [EnumMember(Value = "index")]
        Index,
        [EnumMember(Value = "create")]
        Create
    }
}