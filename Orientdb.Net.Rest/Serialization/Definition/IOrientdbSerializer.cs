using System.IO;
using System.Threading.Tasks;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.Serialization
// ReSharper restore CheckNamespace
{
	public interface IOrientdbSerializer
	{
		T Deserialize<T>(Stream stream);

		Task<T> DeserializeAsync<T>(Stream stream);

		byte[] Serialize(object data, SerializationFormatting formatting = SerializationFormatting.Indented);

		/// <summary>
		/// Used to stringify valuetypes to string (i.e querystring parameters and route parameters).
		/// </summary>
		string Stringify(object valueType);
	}
}