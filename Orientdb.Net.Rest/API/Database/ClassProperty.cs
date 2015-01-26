// ReSharper disable CheckNamespace
namespace Orientdb.Net.API
// ReSharper restore CheckNamespace
{
    public class ClassProperty
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public bool Mandatory { get; set; }

        public bool Readonly { get; set; }

        public bool NotNull { get; set; }

        public int? Min { get; set; }

        public int? Max { get; set; }

        public string Collate { get; set; }
    }
}