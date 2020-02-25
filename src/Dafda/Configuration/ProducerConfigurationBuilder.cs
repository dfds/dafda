using System;
using System.Collections.Generic;
using System.Linq;
using Dafda.Logging;
using Dafda.Producing;

namespace Dafda.Configuration
{
    public sealed class ProducerConfigurationBuilder
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private static readonly string[] DefaultConfigurationKeys =
        {
            ConfigurationKey.BootstrapServers,
            ConfigurationKey.BrokerVersionFallback,
            ConfigurationKey.ApiVersionFallbackMs,
            ConfigurationKey.SslCaLocation,
            ConfigurationKey.SaslUsername,
            ConfigurationKey.SaslPassword,
            ConfigurationKey.SaslMechanisms,
            ConfigurationKey.SecurityProtocol,
        };

        private static readonly string[] RequiredConfigurationKeys =
        {
            ConfigurationKey.BootstrapServers
        };

        private readonly IDictionary<string, string> _configurations = new Dictionary<string, string>();
        private readonly IList<NamingConvention> _namingConventions = new List<NamingConvention>();

        private ConfigurationSource _configurationSource = ConfigurationSource.Null;
        private MessageIdGenerator _messageIdGenerator = MessageIdGenerator.Default;
        private Func<KafkaProducer> _kafkaProducerFactory;

        internal ProducerConfigurationBuilder()
        {
        }

        public ProducerConfigurationBuilder WithConfigurationSource(ConfigurationSource configurationSource)
        {
            _configurationSource = configurationSource;
            return this;
        }

        public ProducerConfigurationBuilder WithNamingConvention(Func<string, string> converter)
        {
            _namingConventions.Add(NamingConvention.UseCustom(converter));
            return this;
        }

        internal ProducerConfigurationBuilder WithNamingConvention(NamingConvention namingConvention)
        {
            _namingConventions.Add(namingConvention);
            return this;
        }

        public ProducerConfigurationBuilder WithEnvironmentStyle(string prefix = null, params string[] additionalPrefixes)
        {
            WithNamingConvention(NamingConvention.UseEnvironmentStyle(prefix));

            foreach (var additionalPrefix in additionalPrefixes)
            {
                WithNamingConvention(NamingConvention.UseEnvironmentStyle(additionalPrefix));
            }

            return this;
        }

        public ProducerConfigurationBuilder WithConfiguration(string key, string value)
        {
            _configurations[key] = value;
            return this;
        }

        public ProducerConfigurationBuilder WithBootstrapServers(string bootstrapServers)
        {
            return WithConfiguration(ConfigurationKey.BootstrapServers, bootstrapServers);
        }

        public ProducerConfigurationBuilder WithMessageIdGenerator(MessageIdGenerator messageIdGenerator)
        {
            _messageIdGenerator = messageIdGenerator;
            return this;
        }

        internal ProducerConfigurationBuilder WithKafkaProducerFactory(Func<KafkaProducer> inlineFactory)
        {
            _kafkaProducerFactory = inlineFactory;
            return this;
        }

        internal ProducerConfiguration Build()
        {
            if (!_namingConventions.Any())
            {
                _namingConventions.Add(item: NamingConvention.Default);
            }

            FillConfiguration();
            ValidateConfiguration();

            if (_kafkaProducerFactory == null)
            {
                _kafkaProducerFactory = () => new KafkaProducer(_configurations);
            }

            return new ProducerConfiguration(
                _configurations,
                _messageIdGenerator,
                _kafkaProducerFactory
            );
        }

        private void FillConfiguration()
        {
            foreach (var key in AllKeys)
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

        private static IEnumerable<string> AllKeys => DefaultConfigurationKeys.Concat(RequiredConfigurationKeys).Distinct();

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
            foreach (var key in RequiredConfigurationKeys)
            {
                if (!_configurations.TryGetValue(key, out var value) || string.IsNullOrEmpty(value))
                {
                    var message = $"Expected key '{key}' not supplied in '{GetSourceName()}' (attempted keys: '{string.Join("', '", GetAttemptedKeys(key))}')";
                    throw new InvalidConfigurationException(message);
                }
            }
        }
    }
}