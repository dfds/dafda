using System;
using System.Collections.Generic;
using Dafda.Outbox;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public sealed class ProducerOptions
    {
        private readonly ProducerConfigurationBuilder _builder;
        private readonly IServiceCollection _services;
        private readonly OutgoingMessageRegistry _outgoingMessageRegistry;

        internal ProducerOptions(ProducerConfigurationBuilder builder, IServiceCollection services, OutgoingMessageRegistry outgoingMessageRegistry)
        {
            _builder = builder;
            _services = services;
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

        internal void WithKafkaProducerFactory(Func<IKafkaProducer> inlineFactory)
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

        //public void AddOutbox(Action<IOutboxOptions> config)
        //{
        //    var configuration = new OutboxOptions(_services);
        //    config?.Invoke(configuration);

        //    _services.AddTransient<IOutbox>(provider =>
        //    {
        //        var producerConfiguration = provider.GetRequiredService<ProducerConfiguration>();
        //        var repository = provider.GetRequiredService<IOutboxMessageRepository>();
        //        return new OutboxMessageCollector(producerConfiguration.MessageIdGenerator, producerConfiguration.OutgoingMessageRegistry, repository);
        //    });
        //}

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