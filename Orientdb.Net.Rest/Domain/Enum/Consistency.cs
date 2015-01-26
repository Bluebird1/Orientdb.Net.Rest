using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public enum Consistency
    {
        [EnumMember(Value = "one")]
        One,
        [EnumMember(Value = "quorum")]
        Quorum,
        [EnumMember(Value = "all")]
        All
    }
}