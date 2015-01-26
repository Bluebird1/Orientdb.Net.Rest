using System;
using System.Collections.Generic;
using System.IO;
using Orientdb.Net.Connection;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public abstract class FluentRequestParameters<T> : IRequestParameters
        where T : FluentRequestParameters<T>
    {
        public FluentRequestParameters()
        {
            Self.QueryString = new Dictionary<string, object>();
        }

        private IRequestParameters Self
        {
            get { return this; }
        }

        IDictionary<string, object> IRequestParameters.QueryString { get; set; }
        Func<IOrientdbResponse, Stream, object> IRequestParameters.DeserializationState { get; set; }
        IRequestConfiguration IRequestParameters.RequestConfiguration { get; set; }

        public TOut GetQueryStringValue<TOut>(string name)
        {
            if (!ContainsKey(name))
                return default(TOut);
            object value = Self.QueryString[name];
            if (value == null)
                return default(TOut);
            return (TOut) value;
        }


        public T CopyQueryStringValuesFrom(IRequestParameters requestParameters)
        {
            if (requestParameters == null)
                return (T) this;
            IDictionary<string, object> from = requestParameters.QueryString;
            foreach (string k in from.Keys)
                Self.QueryString[k] = from[k];
            return (T) this;
        }

        public T AddQueryString(string name, object value)
        {
            Self.QueryString[name] = value;
            return (T) this;
        }

        public T RequestConfiguration(Func<IRequestConfiguration, RequestConfigurationDescriptor> selector)
        {
            selector.ThrowIfNull("selector");
            Self.RequestConfiguration = selector(Self.RequestConfiguration ?? new RequestConfigurationDescriptor());
            return (T) this;
        }

        public T DeserializationState(Func<IOrientdbResponse, Stream, object> customResponseCreator)
        {
            Self.DeserializationState = customResponseCreator;
            return (T) this;
        }

        public bool ContainsKey(string name)
        {
            return Self.QueryString != null && Self.QueryString.ContainsKey(name);
        }

        public T RemoveQueryString(string name)
        {
            Self.QueryString.Remove(name);
            return (T) this;
        }
    }
}