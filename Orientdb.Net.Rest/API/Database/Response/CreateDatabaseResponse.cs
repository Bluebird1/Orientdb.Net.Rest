using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.API
// ReSharper restore CheckNamespace
{

    public interface ICreateDatabaseResponse : IResponse
    {

        List<OrienClass> Classes { get; set; }

        List<OrienCluster> Clusters { get; set; }

        string CurrentUser { get; set; }

        List<OrienUser> Users { get; set; }
    }


    public class CreateDatabaseResponse : BaseResponse, ICreateDatabaseResponse
    {
        public CreateDatabaseResponse()
        {
            IsValid = true;
        }

        [JsonProperty("classes")]
        public List<OrienClass> Classes { get; set; }

        [JsonProperty("clusters")]
        public List<OrienCluster> Clusters { get; set; }

        [JsonProperty("currentUser")]
        public string CurrentUser { get; set; }

        [JsonProperty("users")]
        public List<OrienUser> Users { get; set; }
    }
}