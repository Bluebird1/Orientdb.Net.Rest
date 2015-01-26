using System;
using Newtonsoft.Json;
using Orientdb.Net.API;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public class OridConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = (string)serializer.Deserialize(reader, typeof(string));
            return new ORID(value);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ORID).IsAssignableFrom(objectType);
        }
    }
}