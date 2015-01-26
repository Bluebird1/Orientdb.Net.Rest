using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public enum WaitForStatus
    {
        [EnumMember(Value = "green")]
        Green,
        [EnumMember(Value = "yellow")]
        Yellow,
        [EnumMember(Value = "red")]
        Red
    }
}