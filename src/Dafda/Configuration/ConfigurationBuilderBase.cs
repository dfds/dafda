using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dafda.Logging;

namespace Dafda.Configuration
{
    public abstract class ConfigurationBuilderBase
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IDictionary<string, string> _configurations = new Dictionary<string, string>();
        private readonly IList<NamingConvention> _namingConventions = new List<NamingConvention>();

        private ConfigurationSource _configurationSource = ConfigurationSource.Null;

        private protected ConfigurationBuilderBase()
        {
        }

        protected void SetConfigurationValue(string key, string value)
        {
            _configurations[key] = value;
        }

        protected void SetConfigurationSource(ConfigurationSource configurationSource)
        {
            _configurationSource = configurationSource;
        }

        protected void AddNamingConvention(NamingConvention namingConvention)
        {
            _namingConventions.Add(namingConvention);
        }

        public IConfiguration Build()
        {
            if (!_namingConventions.Any())
            {
                _namingConventions.Add(NamingConvention.Default);
            }

            FillConfiguration();

            ValidateConfiguration();

            return DictionaryConfiguration.Create(_configurations);
        }

        private void FillConfiguration()
        {
            foreach (var key in AllKeys())
            {
                if (_configurations.ContainsKey(key))
                {
                    continue;
                }

                var value = GetByKey(key);
                if (value != null)
                {
                    _configurations[key] = value;
                }
            }
        }

        private IEnumerable<string> AllKeys()
        {
            return GetDefaultConfigurationKeys().Concat(GetRequiredConfigurationKeys()).Distinct();
        }

        protected abstract IEnumerable<string> GetRequiredConfigurationKeys();

        protected abstract IEnumerable<string> GetDefaultConfigurationKeys();

        private string GetByKey(string key)
        {
            Logger.Debug("Looking for {Key} in {SourceName} using keys {AttemptedKeys}", key, GetSourceName(), GetAttemptedKeys(key));

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

        private void ValidateConfiguration()
        {
            foreach (var key in GetRequiredConfigurationKeys())
            {
                if (!_configurations.TryGetValue(key, out var value) || string.IsNullOrEmpty(value))
                {
                    var message = $"Expected key '{key}' not supplied in '{GetSourceName()}' (attempted keys: '{string.Join("', '", GetAttemptedKeys(key))}')";
                    throw new InvalidConfigurationException(message);
                }
            }
        }

        private class DictionaryConfiguration : IConfiguration
        {
            public static DictionaryConfiguration Create(IDictionary<string, string> configuration)
            {
                return new DictionaryConfiguration(configuration);
            }

            private readonly IDictionary<string, string> _configuration;

            private DictionaryConfiguration(IDictionary<string, string> configuration)
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