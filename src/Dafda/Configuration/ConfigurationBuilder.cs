using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dafda.Logging;

namespace Dafda.Configuration
{
    public delegate string KeyConverter(string keyName);

    public class ConfigurationBuilder
    {
        private const string ConfigurationKeyGroupId = "group.id";
        private const string ConfigurationKeyBootstrapServers = "bootstrap.servers";
        private const string ConfigurationKeyEnableAutoCommit = "enable.auto.commit";

        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        public static readonly KeyConverter PassThroughKeyConverter = key => key;
        public static readonly KeyConverter EnvVarStyleKeyConverter = key => key.ToUpper().Replace('.', '_');

        public static KeyConverter EnvVarStyleKeyConverterWithPrefix(string prefix)
        {
            return keyName => EnvVarStyleKeyConverter(prefix + "_" + keyName);
        }

        private static readonly string[] RequiredConfigurationKeys =
        {
            ConfigurationKeyGroupId,
            ConfigurationKeyBootstrapServers
        };

        public static readonly string[] DefaultConfigurationKeys =
        {
            ConfigurationKeyGroupId,
            ConfigurationKeyEnableAutoCommit,
            ConfigurationKeyBootstrapServers,
            "broker.version.fallback",
            "api.version.fallback.ms",
            "ssl.ca.location",
            "sasl.username",
            "sasl.password",
            "sasl.mechanisms",
            "security.protocol",
        };

        private readonly Dictionary<string, string> _configurations = new Dictionary<string, string>();
        private readonly IList<NamingConvention> _namingConventions = new List<NamingConvention>();

        public ConfigurationBuilder WithConfigurationProvider(IConfigurationProvider configurationProvider, KeyConverter keyConverter = null)
        {
            _configurationProvider = configurationProvider;

            if (keyConverter != null)
            {
                return WithNamingConvention(NamingConvention.UseCustom(s => keyConverter(s)));
            }

            return this;
        }

        private IConfigurationProvider _configurationProvider = ConfigurationProvider.Null;

        public ConfigurationBuilder WithConfigurationProvider(ConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider ?? ConfigurationProvider.Null;
            return this;
        }

        public ConfigurationBuilder WithNamingConvention(NamingConvention namingConvention)
        {
            _namingConventions.Add(namingConvention);
            return this;
        }

        public ConfigurationBuilder WithNamingConvention(Func<string, string> namingConvention)
        {
            return WithNamingConvention(NamingConvention.UseCustom(namingConvention));
        }

        public ConfigurationBuilder WithEnvironmentNamingConvention(string prefix = null)
        {
            return WithNamingConvention(NamingConvention.UseEnvironmentStyle(prefix));
        }

        public ConfigurationBuilder WithGroupId(string groupId)
        {
            return WithConfiguration(ConfigurationKeyGroupId, groupId);
        }

        public ConfigurationBuilder WithBootstrapServers(string bootstrapServers)
        {
            return WithConfiguration(ConfigurationKeyBootstrapServers, bootstrapServers);
        }

        public ConfigurationBuilder WithBootstrapServers(params string[] bootstrapServers)
        {
            return WithBootstrapServers(string.Join(",", bootstrapServers));
        }

        public ConfigurationBuilder WithEnabledAutoCommit(bool enableAutoCommit)
        {
            return WithConfiguration(ConfigurationKeyEnableAutoCommit, enableAutoCommit ? "true" : "false");
        }

        public ConfigurationBuilder WithConfiguration(string key, string value)
        {
            _configurations[key] = value;
            return this;
        }

        public IConfiguration Build()
        {
            if (_namingConventions.Count == 0)
            {
                _namingConventions.Add(NamingConvention.Default);
            }

            foreach (var key in DefaultConfigurationKeys)
            {
                if (_configurations.ContainsKey(key))
                {
                    continue;
                }

                Logger.Debug("Looking for {Key} in {ProviderName} using keys {AttemptedKeys}", key, GetProviderName(), GetAttemptedKeys(key));

                var value = _namingConventions
                    .Select(namingConvention => namingConvention.GetKey(key))
                    .Select(actualKey => _configurationProvider.GetByKey(actualKey))
                    .FirstOrDefault(value1 => value1 != null);

                if (value != null)
                {
                    _configurations[key] = value;
                }
            }

            foreach (var key in RequiredConfigurationKeys)
            {
                if (!_configurations.TryGetValue(key, out var value) || string.IsNullOrEmpty(value))
                {
                    var message = $"Expected key '{key}' not supplied in '{GetProviderName()}' (attempted keys: '{string.Join("', '", GetAttemptedKeys(key))}')";
                    throw new InvalidConfigurationException(message);
                }
            }

            return new DictionaryConfiguration(_configurations);
        }

        private string GetProviderName()
        {
            return _configurationProvider.GetType().Name;
        }

        private IEnumerable<string> GetAttemptedKeys(string key)
        {
            return _namingConventions.Select(convention => convention.GetKey(key));
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

    public class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(string message) : base(message)
        {
        }
    }
}