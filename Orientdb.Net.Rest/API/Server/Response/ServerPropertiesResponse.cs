using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.API
// ReSharper restore CheckNamespace
{

    public interface IGetServerPropertiesResponse : IResponse
    {
        List<OrienConnection> Connections { get; set; }

        string[] Dbs { get; set; }

        List<Storage> Storages { get; set; }

        List<ServerProperties> ServerProperties { get; set; }
    }

    public class GetServerPropertiesResponse : BaseResponse, IGetServerPropertiesResponse
    {
        public GetServerPropertiesResponse()
        {
            IsValid = true;
        }

        [JsonProperty("connections")]
        public List<OrienConnection> Connections { get; set; }

        [JsonProperty("dbs")]
        public string[] Dbs { get; set; }

        [JsonProperty("storages")]
        public List<Storage> Storages { get; set; }

        [JsonProperty("properties")]
        public List<ServerProperties> ServerProperties { get; set; }
    }
}