using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public enum DefaultOperator
    {
        [EnumMember(Value = "AND")]
        And,
        [EnumMember(Value = "OR")]
        Or
    }
}