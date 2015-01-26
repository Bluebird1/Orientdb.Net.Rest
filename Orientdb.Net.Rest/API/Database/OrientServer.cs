// ReSharper disable CheckNamespace
namespace Orientdb.Net.API
// ReSharper restore CheckNamespace
{
    public class OrientServer 
    {
        public string Version { get; set; }

        public string Build { get; set; }

        public string OsName { get; set; }

        public string OsVersion { get; set; }

        public string OsArch { get; set; }

        public string JavaVendor { get; set; }

        public string JavaVersion { get; set; }
    }
}