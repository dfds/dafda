using System;
using Dafda.Producing;
using Dafda.Producing.Kafka;

namespace Dafda.Configuration
{
    public interface IProducerOptions
    {
        void WithConfigurationSource(ConfigurationSource configurationSource);
        void WithConfigurationSource(Microsoft.Extensions.Configuration.IConfiguration configuration);
        void WithNamingConvention(NamingConvention namingConvention);
        void WithEnvironmentStyle(string prefix = null, params string[] additionalPrefixes);
        void WithConfiguration(string key, string value);
        void WithBootstrapServers(string bootstrapServers);

        void WithKafkaProducerFactory(IKafkaProducerFactory kafkaProducerFactory);
        void WithMessageIdGenerator(MessageIdGenerator messageIdGenerator);

        void Register<T>(string topic, string type, Func<T, string> keySelector) where T : class;
    }
    
    internal class ProducerOptions : IProducerOptions
    {
        private readonly ProducerConfigurationBuilder _builder;
        private readonly IOutgoingMessageRegistry _outgoingMessageRegistry;

        public ProducerOptions(ProducerConfigurationBuilder builder, IOutgoingMessageRegistry outgoingMessageRegistry)
        {
            _builder = builder;
            _outgoingMessageRegistry = outgoingMessageRegistry;
        }

        public void WithConfigurationSource(ConfigurationSource configurationSource)
        {
            _builder.WithConfigurationSource(configurationSource);
        }

        public void WithConfigurationSource(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _builder.WithConfigurationSource(new DefaultConfigurationSource(configuration));
        }

        public void WithNamingConvention(NamingConvention namingConvention)
        {
            _builder.WithNamingConvention(namingConvention);
        }

        public void WithEnvironmentStyle(string prefix = null, params string[] additionalPrefixes)
        {
            _builder.WithEnvironmentStyle(prefix, additionalPrefixes);
        }

        public void WithConfiguration(string key, string value)
        {
            _builder.WithConfiguration(key, value);
        }

        public void WithBootstrapServers(string bootstrapServers)
        {
            _builder.WithBootstrapServers(bootstrapServers);
        }

        public void WithKafkaProducerFactory(IKafkaProducerFactory kafkaProducerFactory)
        {
            _builder.WithKafkaProducerFactory(kafkaProducerFactory);
        }

        public void WithMessageIdGenerator(MessageIdGenerator messageIdGenerator)
        {
            _builder.WithMessageIdGenerator(messageIdGenerator);
        }

        public void Register<T>(string topic, string type, Func<T, string> keySelector) where T : class
        {
            _outgoingMessageRegistry.Register(topic, type, keySelector);
        }

        private class DefaultConfigurationSource : ConfigurationSource
        {
            private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

            public DefaultConfigurationSource(Microsoft.Extensions.Configuration.IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public override string GetByKey(string keyName)
            {
                return _configuration[keyName];
            }
        }
    }
}