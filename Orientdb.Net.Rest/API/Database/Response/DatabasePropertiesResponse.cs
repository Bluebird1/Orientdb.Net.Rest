using System.Collections.Generic;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.API
// ReSharper restore CheckNamespace

{
    public interface IGetDatabasePropertiesResponse : IResponse
    {
        OrientServer Server { get; set; }

        List<OrienClass> Classes { get; set; }

        List<OrienCluster> Clusters { get; set; }

        string CurrentUser { get; set; }


        List<OrienUser> Users { get; set; }
    }


    public class GetDatabasePropertiesResponse : BaseResponse, IGetDatabasePropertiesResponse
    {
        public GetDatabasePropertiesResponse()
        {
            IsValid = true;
        }

        public OrientServer Server { get; set; }

        public List<OrienClass> Classes { get; set; }

        public List<OrienCluster> Clusters { get; set; }

        public string CurrentUser { get; set; }

        public List<OrienUser> Users { get; set; }
    }
}