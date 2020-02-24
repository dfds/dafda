using System;
using Dafda.Consuming;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public sealed class ConsumerOptions
    {
        private readonly ConsumerConfigurationBuilder _builder;
        private readonly IServiceCollection _services;

        internal ConsumerOptions(ConsumerConfigurationBuilder builder, IServiceCollection services)
        {
            _services = services;
            _builder = builder;
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

        public void WithGroupId(string groupId)
        {
            _builder.WithGroupId(groupId);
        }

        public void WithIncomingMessageFactory(IIncomingMessageFactory incomingMessageFactory)
        {
            _builder.WithIncomingMessageFactory(incomingMessageFactory);
        }

        public void WithBootstrapServers(string bootstrapServers)
        {
            _builder.WithBootstrapServers(bootstrapServers);
        }

        public void WithUnitOfWorkFactory<T>() where T : class, IHandlerUnitOfWorkFactory
        {
            _services.AddTransient<IHandlerUnitOfWorkFactory, T>();
        }

        public void WithUnitOfWorkFactory(Func<IServiceProvider, IHandlerUnitOfWorkFactory> implementationFactory)
        {
            _services.AddTransient(implementationFactory);
        }

        public void WithConsumerScopeFactory(IConsumerScopeFactory consumerScopeFactory)
        {
            _builder.WithConsumerScopeFactory(consumerScopeFactory);
        }

        public void RegisterMessageHandler<TMessage, TMessageHandler>(string topic, string messageType)
            where TMessage : class, new()
            where TMessageHandler : class, IMessageHandler<TMessage>
        {
            _builder.RegisterMessageHandler<TMessage, TMessageHandler>(topic, messageType);
            _services.AddTransient<TMessageHandler>();
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