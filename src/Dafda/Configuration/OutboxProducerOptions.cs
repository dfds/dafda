using System;
using Dafda.Outbox;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
    public sealed class OutboxProducerOptions
    {
        private readonly ProducerConfigurationBuilder _builder;
        private readonly IServiceCollection _services;

        internal OutboxProducerOptions(ProducerConfigurationBuilder builder, IServiceCollection services)
        {
            _services = services;
            _builder = builder;
        }

        internal IOutboxListener OutboxListener { get; private set; }

        public void WithConfigurationSource(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _builder.WithConfigurationSource(new DefaultConfigurationSource(configuration));
        }

        public void WithConfigurationSource(ConfigurationSource configurationSource)
        {
            _builder.WithConfigurationSource(configurationSource);
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

        public void WithUnitOfWorkFactory<T>() where T : class, IOutboxUnitOfWorkFactory
        {
            _services.AddTransient<IOutboxUnitOfWorkFactory, T>();
        }

        public void WithUnitOfWorkFactory(Func<IServiceProvider, IOutboxUnitOfWorkFactory> implementationFactory)
        {
            _services.AddTransient(implementationFactory);
        }

        public void WithListener(IOutboxListener outboxListener)
        {
            OutboxListener = outboxListener;
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