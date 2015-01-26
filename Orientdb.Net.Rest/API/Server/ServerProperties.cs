using Newtonsoft.Json;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.API
// ReSharper restore CheckNamespace

{
    public class ServerProperties
    {
        [JsonProperty("name")]
        public string ServerPropertiesName { get; set; }

        [JsonProperty("value")]
        public string ServerPropertiesvalue { get; set; }
    }
}