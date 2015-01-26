using Newtonsoft.Json;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.API
// ReSharper restore CheckNamespace

{
    public interface IODocument : IResponse
    {
        // ReSharper disable InconsistentNaming
        ORID ORID { get; set; }
        // ReSharper restore InconsistentNaming

        int OVersion { get; set; }

        string OClassName { get; set; }

        string RecordType { get; set; }
    }


    public class ODocument : BaseResponse, IODocument
    {
        public ODocument()
        {
            IsValid = true;
        }


        [JsonProperty("@rid")]
        //[JsonConverter(typeof(OridConverter))]
        // ReSharper disable InconsistentNaming
        public ORID ORID { get; set; }
        // ReSharper restore InconsistentNaming
        
        [JsonProperty("@version")]
        public int OVersion { get; set; }

        [JsonProperty("@class")]
        public string OClassName { get; set; }

        [JsonProperty("@type")]
        public string RecordType { get; set; }

        [JsonProperty("@fieldTypes")]
        public string FieldTypes { get; set; }
    }
}