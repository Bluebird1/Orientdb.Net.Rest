using System.IO;

namespace Orientdb.Net.Providers
{
    public class MemoryStreamProvider : IMemoryStreamProvider
    {
        public MemoryStream New()
        {
            return new MemoryStream();
        }
    }
}