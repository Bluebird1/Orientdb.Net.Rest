using System;
using Newtonsoft.Json;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public interface ICustomJsonReader<out T> where T : class, new()
    {
        T FromJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer);
    }
}