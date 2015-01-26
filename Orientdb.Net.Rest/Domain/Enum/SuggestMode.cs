using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public enum SuggestMode
    {
        [EnumMember(Value = "missing")]
        Missing,
        [EnumMember(Value = "popular")]
        Popular,
        [EnumMember(Value = "always")]
        Always
    }
}