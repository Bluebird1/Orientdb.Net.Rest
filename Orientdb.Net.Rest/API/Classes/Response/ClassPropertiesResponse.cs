using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.API
// ReSharper restore CheckNamespace
{

    public interface IGetClassPropertiesResponse : IResponse
    {
        string Name { get; set; }

        string SuperClass { get; set; }

        string Alias { get; set; }

        bool Abstract { get; set; }

        bool Strictmode { get; set; }

        int[] Clusters { get; set; }
        
        int DefaultCluster { get; set; }

        string ClusterSelection { get; set; }

        int Records { get; set; }

        List<ClassProperty> Properties { get; set; }

        List<ClassIndex> Indexes { get; set; }
    }

    public class GetClassPropertiesResponse : BaseResponse, IGetClassPropertiesResponse
    {
        public GetClassPropertiesResponse()
        {
            IsValid = true;
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("superClass")]
        public string SuperClass { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("abstract")]
        public bool Abstract { get; set; }

        [JsonProperty("strictmode")]
        public bool Strictmode { get; set; }

        [JsonProperty("clusters")]
        public int[] Clusters { get; set; }

        [JsonProperty("defaultCluster")]
        public int DefaultCluster { get; set; }

        [JsonProperty("clusterSelection")]
        public string ClusterSelection { get; set; }

        [JsonProperty("records")]
        public int Records { get; set; }

        [JsonProperty("properties")]
        public List<ClassProperty> Properties { get; set; }

        [JsonProperty("indexes")]
        public List<ClassIndex> Indexes { get; set; }
    }
}