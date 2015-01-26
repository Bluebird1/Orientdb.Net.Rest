using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Orientdb.Net.API;

namespace Orientdb.Net.Resolvers
{
    public class OrientdbContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            if (objectType == typeof (ORID))
                contract.Converter = new OridConverter();

            if (objectType == typeof (OrientLinkBag))
                contract.Converter = new OrientLinkBagConverter();

            return contract;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> defaultProperties = base.CreateProperties(type, memberSerialization);
            ILookup<string, JsonProperty> lookup = defaultProperties.ToLookup(p => p.PropertyName);

            defaultProperties = PropertiesOf<OrientLinkBag>(type, memberSerialization, defaultProperties, lookup);

            return defaultProperties;
        }

        private IList<JsonProperty> PropertiesOf<T>(Type type, MemberSerialization memberSerialization,
            IList<JsonProperty> defaultProperties, ILookup<string, JsonProperty> lookup, bool append = false)
        {
            if (!typeof (T).IsAssignableFrom(type)) return defaultProperties;
            IEnumerable<JsonProperty> jsonProperties = (
                from i in type.GetInterfaces()
                select base.CreateProperties(i, memberSerialization)
                )
                .SelectMany(interfaceProps => interfaceProps)
                .Where(p => !lookup.Contains(p.PropertyName));
            if (!append)
            {
                foreach (JsonProperty p in jsonProperties)
                {
                    defaultProperties.Add(p);
                }
                return defaultProperties;
            }
            return jsonProperties.Concat(defaultProperties).GroupBy(p => p.PropertyName).Select(g => g.First()).ToList();
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.DefaultValueHandling.HasValue
                && property.DefaultValueHandling.Value == DefaultValueHandling.Ignore
                && !typeof (string).IsAssignableFrom(property.PropertyType)
                && typeof (IEnumerable).IsAssignableFrom(property.PropertyType))
            {
                Predicate<object> shouldSerialize = obj =>
                {
                    var collection = property.ValueProvider.GetValue(obj) as ICollection;
                    return collection == null || collection.Count != 0;
                };
                property.ShouldSerialize = property.ShouldSerialize == null
                    ? shouldSerialize
                    : (o => property.ShouldSerialize(o) && shouldSerialize(o));
            }

            OrientdbProperty att = OrientdbAttributes.Property(member);
            if (att == null) return property;
            if (!att.PropertyMapping.IsNullOrEmpty())
                property.PropertyName = att.PropertyMapping;

            return property;
        }
    }


    public static class OrientdbAttributes
    {
        public static OrientdbProperty Property(MemberInfo info)
        {
            object[] attributes = info.GetCustomAttributes(typeof (OrientdbProperty), true);
            if (attributes.HasAny())
                return ((OrientdbProperty) attributes.First());

            return null;
        }
    }
}