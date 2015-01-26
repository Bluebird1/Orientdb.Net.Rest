using Orientdb.Net.API;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public partial class OrientdbClient
    {
        public IGetServerPropertiesResponse GetServerProperties()
        {
            const string url = "server";
            return DoRequest<GetServerPropertiesResponse>("GET", url).Response;
        }
    }
}
