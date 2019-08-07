using System.Collections;
using System.Collections.Generic;

namespace Dafda.Configuration
{
    public delegate string KeyConverter(string keyName);

    public class ConfigurationBuilder
    {
        public static readonly KeyConverter PassThroughKeyConverter = key => key;
        public static readonly KeyConverter EnvVarStyleKeyConverter = key => key.ToUpper().Replace('.', '_');

        public static KeyConverter EnvVarStyleKeyConverterWithPrefix(string prefix)
        {
            return keyName => EnvVarStyleKeyConverter(prefix + "_" + keyName);
        }

        public static readonly string[] DefaultConfigurationKeys =
        {
            "group.id",
            "enable.auto.commit",
            "bootstrap.servers",
            "broker.version.fallback",
            "api.version.fallback.ms",
            "ssl.ca.location",
            "sasl.username",
            "sasl.password",
            "sasl.mechanisms",
            "security.protocol",
        };

        private readonly Dictionary<string, string> _configurations = new Dictionary<string, string>();
        private readonly IList<IConfigurationProvider> _configurationProviders = new List<IConfigurationProvider>();

        public ConfigurationBuilder WithConfigurationProvider(IConfigurationProvider configurationProvider, KeyConverter keyConverter = null)
        {
            _configurationProviders.Add(new KeyConverterConfigurationProvider(configurationProvider, keyConverter ?? PassThroughKeyConverter));
            return this;
        }

        public ConfigurationBuilder WithConfiguration(string key, string value)
        {
            _configurations[key] = value;
            return this;
        }

        public IConfiguration Build()
        {
            var finalConfiguration = new Dictionary<string, string>();

            foreach (var key in DefaultConfigurationKeys)
            {
                if (finalConfiguration.ContainsKey(key))
                {
                    continue;
                }

                foreach (var myClass in _configurationProviders)
                {
                    var value = myClass.GetByKey(key);

                    if (value != null)
                    {
                        finalConfiguration[key] = value;
                        break;
                    }
                }
            }

            foreach (var cfg in _configurations)
            {
                finalConfiguration[cfg.Key] = cfg.Value;
            }

            return new DictionaryConfiguration(finalConfiguration);
        }

        private class KeyConverterConfigurationProvider : IConfigurationProvider
        {
            private readonly IConfigurationProvider _configurationConfigurationProvider;
            private readonly KeyConverter _keyConverter;

            public KeyConverterConfigurationProvider(IConfigurationProvider configurationProvider, KeyConverter keyConverter)
            {
                _configurationConfigurationProvider = configurationProvider;
                _keyConverter = keyConverter;
            }

            public string GetByKey(string keyName)
            {
                keyName = _keyConverter(keyName);
                return _configurationConfigurationProvider.GetByKey(keyName);
            }
        }

        private class DictionaryConfiguration : IConfiguration
        {
            private readonly IDictionary<string, string> _configuration;

            public DictionaryConfiguration(IDictionary<string, string> configuration)
            {
                _configuration = configuration;
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                return _configuration.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable) _configuration).GetEnumerator();
            }
        }
    }
}