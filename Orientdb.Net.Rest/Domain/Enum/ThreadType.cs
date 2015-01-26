using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public enum ThreadType
    {
        [EnumMember(Value = "cpu")]
        Cpu,
        [EnumMember(Value = "wait")]
        Wait,
        [EnumMember(Value = "block")]
        Block
    }
}