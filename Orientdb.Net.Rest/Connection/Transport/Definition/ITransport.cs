using System.Threading.Tasks;
using Orientdb.Net.Serialization;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.Connection
// ReSharper restore CheckNamespace
{
    public interface ITransport
    {
        IConnectionConfigurationValues Settings { get; }
        IOrientdbSerializer Serializer { get; }

        OrientdbResponse<T> DoRequest<T>(
            string method,
            string path,
            object data = null,
            IRequestParameters requestParameters = null);

        Task<OrientdbResponse<T>> DoRequestAsync<T>(
            string method,
            string path,
            object data = null,
            IRequestParameters requestParameters = null);
    }
}