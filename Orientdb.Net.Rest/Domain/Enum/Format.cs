using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public enum Format
    {
        [EnumMember(Value = "detailed")]
        Detailed,
        [EnumMember(Value = "text")]
        Text
    }
}