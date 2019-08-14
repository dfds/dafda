using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dafda.Logging;

namespace Dafda.Configuration
{
    public abstract class ConfigurationBuilder
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        public static readonly string[] DefaultConfigurationKeys =
        {
            ConfigurationProperties.GroupId,
            ConfigurationProperties.EnableAutoCommit,
            ConfigurationProperties.BootstrapServers,
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

        private IConfigurationProvider _configurationProvider = ConfigurationProvider.Null;

        public ConfigurationBuilder WithConfigurationProvider(IConfigurationProvider configurationProvider)
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

            FillConfiguration();

            ValidateConfiguration();

            return new DictionaryConfiguration(_configurations);
        }

        private void FillConfiguration()
        {
            foreach (var key in GetDefaultConfigurationKeys())
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
        }

        private static IEnumerable<string> GetDefaultConfigurationKeys()
        {
            return DefaultConfigurationKeys;
        }

        private void ValidateConfiguration()
        {
            foreach (var key in GetRequiredConfigurationKeys())
            {
                if (!_configurations.TryGetValue(key, out var value) || string.IsNullOrEmpty(value))
                {
                    var message = $"Expected key '{key}' not supplied in '{GetProviderName()}' (attempted keys: '{string.Join("', '", GetAttemptedKeys(key))}')";
                    throw new InvalidConfigurationException(message);
                }
            }
        }

        protected abstract IEnumerable<string> GetRequiredConfigurationKeys();

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