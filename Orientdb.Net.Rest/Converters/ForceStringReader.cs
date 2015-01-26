using System;
using System.Globalization;
using Newtonsoft.Json;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public class ForceStringReader : JsonConverter
    {
        public override bool CanRead { get { return true; } }
        public override bool CanWrite { get { return false; } }

        public override bool CanConvert(Type objectType)
        {
            return true; //only to be used with attribute or contract registration.
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Date)
            {
                var dateTime = (reader.Value as DateTime?);
                if (!dateTime.HasValue)
                    return null;
                return dateTime.Value.ToString("yyyy-MM-dd'T'HH:mm:ss.fff", CultureInfo.InvariantCulture);
            }
            return reader.Value as string;
        }
    }
}