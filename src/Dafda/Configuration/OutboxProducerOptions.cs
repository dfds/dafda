using System;
using Dafda.Outbox;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public sealed class OutboxProducerOptions
    {
        private readonly ProducerConfigurationBuilder _builder;
        private readonly IServiceCollection _services;
        private readonly OutgoingMessageRegistry _outgoingMessageRegistry;

        internal OutboxProducerOptions(ProducerConfigurationBuilder builder, IServiceCollection services, OutgoingMessageRegistry outgoingMessageRegistry)
        {
            _builder = builder;
            _services = services;
            _outgoingMessageRegistry = outgoingMessageRegistry;
        }

        public TimeSpan DispatchInterval { get; private set; } = TimeSpan.FromSeconds(5);

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

        public void WithOutboxMessageRepository<T>() where T : class, IOutboxMessageRepository
        {
            _services.AddTransient<IOutboxMessageRepository, T>();
        }

        public void WithOutboxMessageRepository(Func<IServiceProvider, IOutboxMessageRepository> implementationFactory)
        {
            _services.AddTransient(implementationFactory);
        }

        public void WithUnitOfWorkFactory<T>() where T : class, IOutboxUnitOfWorkFactory
        {
            _services.AddTransient<IOutboxUnitOfWorkFactory, T>();
        }

        public void WithUnitOfWorkFactory(Func<IServiceProvider, IOutboxUnitOfWorkFactory> implementationFactory)
        {
            _services.AddTransient(implementationFactory);
        }

        public void WithDispatchInterval(TimeSpan interval)
        {
            DispatchInterval = interval;
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