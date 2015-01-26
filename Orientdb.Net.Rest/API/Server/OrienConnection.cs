using System;
using Newtonsoft.Json;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.API
// ReSharper restore CheckNamespace
{
    public class OrienConnection
    {
        [JsonProperty("connectionId")]
        public string ConnectionId { get; set; }

        [JsonProperty("remoteAddress")]
        public string ConnectionRemoteAddress { get; set; }

        [JsonProperty("db")]
        public string ConnectionDatabase { get; set; }

        [JsonProperty("user")]
        public string ConnectionUser { get; set; }

        [JsonProperty("totalRequests")]
        public int ConnectionTotalRequests { get; set; }

        [JsonProperty("commandInfo")]
        public string ConnectionCommandInfo { get; set; }

        [JsonProperty("lastCommandOn")]
        public DateTime ConnectionLastCommandOn { get; set; }

        [JsonProperty("lastCommandInfo")]
        public string ConnectionLastCommandInfo { get; set; }

        [JsonProperty("lastCommandDetail")]
        public string ConnectionLastCommandDetail { get; set; }

        [JsonProperty("lastExecutionTime")]
        public int ConnectionLastExecutionTime { get; set; }

        [JsonProperty("totalWorkingTime")]
        public int ConnectionTotalWorkingTime { get; set; }

        [JsonProperty("connectedOn")]
        public DateTime ConnectionConnectedOn { get; set; }

        [JsonProperty("protocol")]
        public string ConnectionProtocol { get; set; }

        [JsonProperty("clientId")]
        public string ConnectionclientId { get; set; }

        [JsonProperty("driver")]
        public string ConnectionDriver { get; set; }

    }
}