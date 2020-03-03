using System;
using System.Collections.Generic;
using System.Linq;
using Dafda.Producing;

namespace Dafda.Configuration
{
    public sealed class ProducerConfigurationBuilder
    {
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
            var configurations = new ConfigurationBuilder()
                .WithConfigurationKeys(DefaultConfigurationKeys)
                .WithRequiredConfigurationKeys(RequiredConfigurationKeys)
                .WithNamingConventions(_namingConventions.ToArray())
                .WithConfigurationSource(_configurationSource)
                .WithConfigurations(_configurations)
                .Build();

            if (_kafkaProducerFactory == null)
            {
                _kafkaProducerFactory = () => new KafkaProducer(configurations, new DefaultPayloadSerializer());
            }

            return new ProducerConfiguration(
                configurations,
                _messageIdGenerator,
                _kafkaProducerFactory
            );
        }
    }
}