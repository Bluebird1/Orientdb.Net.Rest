using System;
using System.IO;
using System.Threading.Tasks;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.Connection
// ReSharper restore CheckNamespace
{
	public interface IConnection
	{

		Task<OrientdbResponse<Stream>> Get(Uri uri, IRequestConfiguration requestConfiguration = null);
		OrientdbResponse<Stream> GetSync(Uri uri, IRequestConfiguration requestConfiguration = null);

		Task<OrientdbResponse<Stream>> Head(Uri uri, IRequestConfiguration requestConfiguration = null);
		OrientdbResponse<Stream> HeadSync(Uri uri, IRequestConfiguration requestConfiguration = null);

		Task<OrientdbResponse<Stream>> Post(Uri uri, byte[] data, IRequestConfiguration requestConfiguration = null);
		OrientdbResponse<Stream> PostSync(Uri uri, byte[] data, IRequestConfiguration requestConfiguration = null);

		Task<OrientdbResponse<Stream>> Put(Uri uri, byte[] data, IRequestConfiguration requestConfiguration = null);
		OrientdbResponse<Stream> PutSync(Uri uri, byte[] data, IRequestConfiguration requestConfiguration = null);

		Task<OrientdbResponse<Stream>> Delete(Uri uri, IRequestConfiguration requestConfiguration = null);
		OrientdbResponse<Stream> DeleteSync(Uri uri, IRequestConfiguration requestConfiguration = null);

		Task<OrientdbResponse<Stream>> Delete(Uri uri, byte[] data, IRequestConfiguration requestConfiguration = null);
		OrientdbResponse<Stream> DeleteSync(Uri uri, byte[] data, IRequestConfiguration requestConfiguration = null);
		
		
	}
}
