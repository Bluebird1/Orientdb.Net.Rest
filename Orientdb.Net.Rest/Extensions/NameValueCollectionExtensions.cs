using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Orientdb.Net.Serialization;

// ReSharper disable CheckNamespace

namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    internal static class NameValueCollectionExtensions
    {
        internal static string ToQueryString(this NameValueCollection self, string prefix = "?")
        {
            if (self == null)
                return null;

            if (self.AllKeys.Length == 0) return string.Empty;

            return prefix +
                   string.Join("&",
                       Array.ConvertAll(self.AllKeys, key => string.Format("{0}={1}", Encode(key), Encode(self[key]))));
        }

        private static string Encode(string s)
        {
            return s == null ? null : Uri.EscapeDataString(s);
        }

        internal static NameValueCollection ToNameValueCollection(this IDictionary<string, object> dict,
            IOrientdbSerializer stringifier)
        {
            stringifier.ThrowIfNull("stringifier");
            if (dict == null || dict.Count < 0)
                return null;

            var nv = new NameValueCollection();
            foreach (var kv in dict.Where(kv => !kv.Key.IsNullOrEmpty()))
            {
                nv.Add(kv.Key, stringifier.Stringify(kv.Value));
            }
            return nv;
        }
    }
}