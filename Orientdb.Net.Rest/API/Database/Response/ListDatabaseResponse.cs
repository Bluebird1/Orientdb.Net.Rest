using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable CheckNamespace


namespace Orientdb.Net.API
// ReSharper restore CheckNamespace
{

    public interface IListDatabaseResponse : IResponse
    {
        string DatabaseType { get; set; }

        int DatabaseVersion { get; set; }

        string[] Databases { get; set; }

        string DatabaseFieldTypes { get; set; }

    }

    public class ListDatabaseResponse : BaseResponse, IListDatabaseResponse
    {
        public ListDatabaseResponse()
        {
            IsValid = true;
        }

        [JsonProperty("@type")]
        public string DatabaseType { get; set; }

        [JsonProperty("@version")]
        public int DatabaseVersion { get; set; }

        [JsonProperty("databases")]
        public string[] Databases { get; set; }

        [JsonProperty("@fieldTypes")]
        public string DatabaseFieldTypes { get; set; }
    }
}