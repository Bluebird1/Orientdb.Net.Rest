using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public enum Bytes
    {
        [EnumMember(Value = "b")]
        B,
        [EnumMember(Value = "k")]
        K,
        [EnumMember(Value = "m")]
        M,
        [EnumMember(Value = "g")]
        G
    }
}