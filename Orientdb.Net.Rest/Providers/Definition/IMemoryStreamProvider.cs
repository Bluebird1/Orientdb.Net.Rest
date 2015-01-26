using System.IO;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.Providers
// ReSharper restore CheckNamespace
{
    public interface IMemoryStreamProvider
    {
        MemoryStream New();
    }
}