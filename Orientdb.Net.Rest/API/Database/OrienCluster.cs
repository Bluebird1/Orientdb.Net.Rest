// ReSharper disable CheckNamespace
namespace Orientdb.Net.API
// ReSharper restore CheckNamespace
{
    public class OrienCluster
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Records { get; set; }

        public string ConflictStrategy { get; set; }

        public string Size { get; set; }

        public string Filled { get; set; }

        public string MaxSize { get; set; }

        public string Files { get; set; }
    }
}