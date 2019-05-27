using System;
using System.Collections;
using System.Collections.Generic;

namespace Dafda.Configuration
{
    public class ConfigurationBuilder
    {
        public static readonly Func<string, string> PassthroughKeyConverter = key => key;
        public static readonly Func<string, string> EnvVarStyleKeyConverter = key => key.ToUpper().Replace('.', '_');
        
        public static readonly string[] DefaultConfigurationKeys = new[]
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
        private IConfigurationProvider _configurationProvider;
        private Func<string, string> _keyConverter = key => key;

        private void AddOrUpdate(IDictionary<string, string> dictionary, string key, string value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }            
        }
        
        public ConfigurationBuilder WithConfigurationProvider(IConfigurationProvider configurationProvider, Func<string, string> keyConverter = null)
        {
            _configurationProvider = configurationProvider;

            if (keyConverter != null)
            {
                _keyConverter = keyConverter;
            }
            
            return this;
        }

        public ConfigurationBuilder WithConfiguration(string key, string value)
        {
            AddOrUpdate(_configurations, key, value);
            return this;
        }

        public IConfiguration Build()
        {
            var finalConfiguration = new Dictionary<string, string>();

            if (_configurationProvider != null)
            {
                foreach (var key in DefaultConfigurationKeys)
                {
                    if (finalConfiguration.ContainsKey(key))
                    {
                        continue;
                    }
                    
                    var actualKey = _keyConverter(key);
                    var value = _configurationProvider.GetByKey(actualKey);

                    if (value != null)
                    {
                        AddOrUpdate(finalConfiguration, key, value);
                    }
                }
            }
            
            foreach (var cfg in _configurations)
            {
                AddOrUpdate(finalConfiguration, cfg.Key, cfg.Value);
            }
            
            return new DictionaryConfiguration(finalConfiguration);
        }

        private class DictionaryConfiguration : IConfiguration
        {
            private readonly Dictionary<string, string> _configuration;

            public DictionaryConfiguration(Dictionary<string, string> configuration)
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