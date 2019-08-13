using System.Collections.Generic;
using System.Collections.Immutable;
using Dafda.Configuration;

namespace Dafda.Tests.TestDoubles
{
    public class ConfigurationProviderStub : ConfigurationProvider
    {
        private readonly IDictionary<string, string> _configuration;

        public ConfigurationProviderStub(IDictionary<string, string> configuration = null)
        {
            _configuration = configuration ?? ImmutableDictionary<string, string>.Empty;
        }

        public override string GetByKey(string keyName)
        {
            _configuration.TryGetValue(keyName, out var value);
            return value;
        }
    }
}