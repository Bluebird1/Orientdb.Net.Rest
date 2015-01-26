using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public enum ExpandWildcards
    {
        [EnumMember(Value = "open")]
        Open,
        [EnumMember(Value = "closed")]
        Closed
    }
}