using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public enum VersionType
    {
        [EnumMember(Value = "internal")]
        Internal,
        [EnumMember(Value = "external")]
        External,
        [EnumMember(Value = "external_gte")]
        ExternalGte,
        [EnumMember(Value = "force")]
        Force
    }
}