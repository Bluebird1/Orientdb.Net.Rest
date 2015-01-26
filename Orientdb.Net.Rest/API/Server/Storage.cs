using Newtonsoft.Json;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.API
// ReSharper restore CheckNamespace
{
    public class Storage
    {
        [JsonProperty("name")]
        public string StorageName { get; set; }

        [JsonProperty("type")]
        public string StorageType { get; set; }

        [JsonProperty("path")]
        public string StoragePath { get; set; }

        [JsonProperty("activeUsers")]
        public int ActiveUsers { get; set; }
    }
}