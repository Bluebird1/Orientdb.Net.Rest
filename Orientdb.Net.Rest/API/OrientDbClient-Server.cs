using Orientdb.Net.API;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public partial class OrientdbClient
    {
        /// <summary>
        /// Retrieve information about the connected OrientDB Server. Requires additional authentication to the server.
        /// </summary>
        /// <remarks>
        /// Syntax: http://&lt;server&gt;:[&lt;port&gt;]/server
        /// </remarks>
        /// <see cref="http://www.orientechnologies.com/docs/last/orientdb.wiki/OrientDB-REST.html#server"/>
        public IGetServerPropertiesResponse GetServerProperties()
        {
            const string url = "server";
            return DoRequest<GetServerPropertiesResponse>("GET", url).Response;
        }
    }
}
