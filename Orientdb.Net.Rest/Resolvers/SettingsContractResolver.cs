using System;
using Newtonsoft.Json.Serialization;

namespace Orientdb.Net.Resolvers
{
    public class SettingsContractResolver : IContractResolver
    {
        private readonly IContractResolver _wrapped;

        public SettingsContractResolver(IContractResolver wrapped)
        {
            _wrapped = wrapped ?? new DefaultContractResolver();
        }

        /// <summary>
        ///     Signals to custom converter that it can get serialization state from one of the converters
        ///     Ugly but massive performance gain
        /// </summary>
        internal JsonConverterPiggyBackState PiggyBackState { get; set; }

        public JsonContract ResolveContract(Type type)
        {
            return _wrapped.ResolveContract(type);
        }
    }
}