using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Orientdb.Net.Resolvers;

namespace Orientdb.Net.Serialization
{
    public class OrientdbSerializer : IOrientdbSerializer
    {
        private readonly JsonSerializerSettings _serializationSettings;

        public OrientdbSerializer()
        {
            _serializationSettings = CreateSettings();
        }

        public T Deserialize<T>(Stream stream)
        {
            if (stream == null) return default(T);
            JsonSerializerSettings settings = _serializationSettings;
            return DeserializeUsingSettings<T>(stream, settings);
        }

        public Task<T> DeserializeAsync<T>(Stream stream)
        {
            var tcs = new TaskCompletionSource<T>();
            if (stream == null)
            {
                tcs.SetResult(default(T));
                return tcs.Task;
            }
            var r = Deserialize<T>(stream);
            tcs.SetResult(r);
            return tcs.Task;
        }

        public byte[] Serialize(object data, SerializationFormatting formatting = SerializationFormatting.Indented)
        {
            Formatting format = formatting == SerializationFormatting.None ? Formatting.None : Formatting.Indented;
            string serialized = JsonConvert.SerializeObject(data, format, _serializationSettings);
            return serialized.Utf8Bytes();
        }

        public string Stringify(object valueType)
        {
            return DefaultStringify(valueType);
        }

        public static string DefaultStringify(object valueType)
        {
            var s = valueType as string;
            if (s != null)
                return s;
            var ss = valueType as string[];
            if (ss != null)
                return string.Join(",", ss);

            var pns = valueType as IEnumerable<object>;
            if (pns != null)
                return string.Join(",", pns);

            var e = valueType as Enum;
            if (e != null) return KnownEnums.Resolve(e);
            if (valueType is bool)
                return ((bool) valueType) ? "true" : "false";
            return valueType.ToString();
        }

        private T DeserializeUsingSettings<T>(Stream stream, JsonSerializerSettings settings = null)
        {
            if (stream == null) return default(T);
            settings = settings ?? _serializationSettings;
            JsonSerializer serializer = JsonSerializer.Create(settings);
            var jsonTextReader = new JsonTextReader(new StreamReader(stream));
            var t = (T) serializer.Deserialize(jsonTextReader, typeof (T));
            return t;
        }

        internal JsonSerializerSettings CreateSettings(JsonConverter piggyBackJsonConverter = null)
        {
            var piggyBackState = new JsonConverterPiggyBackState { ActualJsonConverter = piggyBackJsonConverter };
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new OrientdbContractResolver(),
                DefaultValueHandling = DefaultValueHandling.Include,
                NullValueHandling = NullValueHandling.Ignore
            };
            settings.ContractResolver = new SettingsContractResolver(settings.ContractResolver) { PiggyBackState = piggyBackState };

            return settings;
        }
    }
}