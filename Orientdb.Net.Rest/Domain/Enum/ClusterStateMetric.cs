using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public enum ClusterStateMetric
    {
        [EnumMember(Value = "_all")]
        All,
        [EnumMember(Value = "blocks")]
        Blocks,
        [EnumMember(Value = "metadata")]
        Metadata,
        [EnumMember(Value = "nodes")]
        Nodes,
        [EnumMember(Value = "routing_table")]
        RoutingTable,
        [EnumMember(Value = "master_node")]
        MasterNode,
        [EnumMember(Value = "version")]
        Version
    }
}