using System;
using Dafda.Producing;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
    public sealed class ProducerOptions
    {
        private readonly ProducerConfigurationBuilder _builder;
        private readonly OutgoingMessageRegistry _outgoingMessageRegistry;

        internal ProducerOptions(ProducerConfigurationBuilder builder, OutgoingMessageRegistry outgoingMessageRegistry)
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

        public void WithNamingConvention(Func<string, string> converter)
        {
            _builder.WithNamingConvention(converter);
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

        internal void WithKafkaProducerFactory(Func<ILoggerFactory, KafkaProducer> inlineFactory)
        {
            _builder.WithKafkaProducerFactory(inlineFactory);
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

            public override string GetByKey(string key)
            {
                return _configuration[key];
            }
        }
    }
}