using System.Collections.Generic;
using System.Linq;

namespace Dafda.Configuration
{
    internal class ConfigurationBuilder
    {
        private static readonly string[] EmptyConfigurationKeys = new string[0];
        private static readonly NamingConvention[] DefaultNamingConventions = {NamingConvention.Default};

        private readonly string[] _configurationKeys;
        private readonly string[] _requiredConfigurationKeys;
        private readonly NamingConvention[] _namingConventions;
        private readonly ConfigurationSource _configurationSource;
        private readonly IDictionary<string, string> _configurations;

        public ConfigurationBuilder()
            : this(EmptyConfigurationKeys, EmptyConfigurationKeys, DefaultNamingConventions, ConfigurationSource.Null, new Dictionary<string, string>())
        {
        }

        private ConfigurationBuilder(string[] configurationKeys, string[] requiredConfigurationKeys, NamingConvention[] namingConventions, ConfigurationSource configurationSource, IDictionary<string, string> configurations)
        {
            _configurationKeys = configurationKeys;
            _requiredConfigurationKeys = requiredConfigurationKeys;
            _namingConventions = namingConventions.Length == 0 ? DefaultNamingConventions : namingConventions;
            _configurationSource = configurationSource;
            _configurations = configurations;
        }

        public ConfigurationBuilder WithConfigurationKeys(params string[] configurationKeys)
        {
            return new ConfigurationBuilder(configurationKeys, _requiredConfigurationKeys, _namingConventions, _configurationSource, _configurations);
        }

        public ConfigurationBuilder WithRequiredConfigurationKeys(params string[] requiredConfigurationKeys)
        {
            return new ConfigurationBuilder(_configurationKeys, requiredConfigurationKeys, _namingConventions, _configurationSource, _configurations);
        }

        public ConfigurationBuilder WithNamingConventions(params NamingConvention[] namingConventions)
        {
            return new ConfigurationBuilder(_configurationKeys, _requiredConfigurationKeys, namingConventions, _configurationSource, _configurations);
        }

        public ConfigurationBuilder WithConfigurationSource(ConfigurationSource configurationSource)
        {
            return new ConfigurationBuilder(_configurationKeys, _requiredConfigurationKeys, _namingConventions, configurationSource, _configurations);
        }

        public ConfigurationBuilder WithConfigurations(IDictionary<string, string> configurations)
        {
            return new ConfigurationBuilder(_configurationKeys, _requiredConfigurationKeys, _namingConventions, _configurationSource, configurations);
        }

        public IDictionary<string, string> Build()
        {
            var configurations = FillConfiguration();

            ValidateConfiguration(configurations);

            return configurations;
        }

        private IDictionary<string, string> FillConfiguration()
        {
            var configurations = new Dictionary<string, string>(_configurations);

            foreach (var key in AllKeys)
            {
                if (configurations.ContainsKey(key))
                {
                    continue;
                }

                var value = GetByKey(key);
                if (value != null)
                {
                    configurations[key] = value;
                }
            }

            return configurations;
        }

        private IEnumerable<string> AllKeys => _configurationKeys.Concat(_requiredConfigurationKeys).Distinct();

        private string GetByKey(string key)
        {
            //ConsumerConfigurationBuilder.Logger.LogDebug("Looking for {Key} in {SourceName} using keys {AttemptedKeys}", key, GetSourceName(), GetAttemptedKeys(key));

            return _namingConventions
                .Select(namingConvention => namingConvention.GetKey(key))
                .Select(actualKey => _configurationSource.GetByKey(actualKey))
                .FirstOrDefault(value => value != null);
        }

        private string GetSourceName()
        {
            return _configurationSource.GetType().Name;
        }

        private IEnumerable<string> GetAttemptedKeys(string key)
        {
            return _namingConventions.Select(convention => convention.GetKey(key));
        }

        private void ValidateConfiguration(IDictionary<string, string> configurations)
        {
            foreach (var key in _requiredConfigurationKeys)
            {
                if (!configurations.TryGetValue(key, out var value) || string.IsNullOrEmpty(value))
                {
                    var message = $"Expected key '{key}' not supplied in '{GetSourceName()}' (attempted keys: '{string.Join("', '", GetAttemptedKeys(key))}')";
                    throw new InvalidConfigurationException(message);
                }
            }
        }
    }
}