using System;
using System.Collections.Generic;
using System.IO;
using Orientdb.Net.Connection;

// ReSharper disable CheckNamespace

namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public class RequestParameters : IRequestParameters
    {
        public RequestParameters()
        {
            QueryString = new Dictionary<string, object>();
        }

        public IDictionary<string, object> QueryString { get; set; }
        public Func<IOrientdbResponse, Stream, object> DeserializationState { get; set; }
        public IRequestConfiguration RequestConfiguration { get; set; }

        public TOut GetQueryStringValue<TOut>(string name)
        {
            if (!QueryString.ContainsKey(name))
                return default(TOut);
            object value = QueryString[name];
            if (value == null)
                return default(TOut);
            return (TOut) value;
        }

        public bool ContainsKey(string name)
        {
            return QueryString != null && QueryString.ContainsKey(name);
        }
    }
}