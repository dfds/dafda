using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public interface IConsumerConfigurationBuilder
    {
        void WithConfigurationSource(ConfigurationSource configurationSource);
        void WithConfigurationSource(Microsoft.Extensions.Configuration.IConfiguration configuration);
        void WithNamingConvention(NamingConvention namingConvention);
        void WithEnvironmentStyle(string prefix = null, params string[] additionalPrefixes);
        void WithConfiguration(string key, string value);
        void WithGroupId(string groupId);
        void WithBootstrapServers(string bootstrapServers);

        void WithUnitOfWorkFactory<T>() where T : class, IHandlerUnitOfWorkFactory;
        void WithUnitOfWorkFactory(IHandlerUnitOfWorkFactory unitOfWorkFactory);
        void WithUnitOfWorkFactory(Func<IServiceProvider, IHandlerUnitOfWorkFactory> implementationFactory);

        void WithTopicSubscriberScopeFactory(ITopicSubscriberScopeFactory topicSubscriberScopeFactory);

        void RegisterMessageHandler<TMessage, TMessageHandler>(string topic, string messageType)
            where TMessage : class, new()
            where TMessageHandler : class, IMessageHandler<TMessage>;
    }

    public class ServiceConsumerConfigurationBuilder : IConsumerConfigurationBuilder
    {
        private readonly ConsumerConfigurationBuilder _configurationBuilder = new ConsumerConfigurationBuilder();
        private readonly IServiceCollection _services;

        public ServiceConsumerConfigurationBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public void WithConfigurationSource(ConfigurationSource configurationSource)
        {
            _configurationBuilder.WithConfigurationSource(configurationSource);
        }

        public void WithConfigurationSource(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _configurationBuilder.WithConfigurationSource(new DefaultConfigurationSource(configuration));
        }

        public void WithNamingConvention(NamingConvention namingConvention)
        {
            _configurationBuilder.WithNamingConvention(namingConvention);
        }

        public void WithEnvironmentStyle(string prefix = null, params string[] additionalPrefixes)
        {
            _configurationBuilder.WithEnvironmentStyle(prefix, additionalPrefixes);
        }

        public void WithConfiguration(string key, string value)
        {
            _configurationBuilder.WithConfiguration(key, value);
        }

        public void WithGroupId(string groupId)
        {
            _configurationBuilder.WithGroupId(groupId);
        }

        public void WithBootstrapServers(string bootstrapServers)
        {
            _configurationBuilder.WithBootstrapServers(bootstrapServers);
        }

        public void WithUnitOfWorkFactory<T>() where T : class, IHandlerUnitOfWorkFactory
        {
            _services.AddTransient<IHandlerUnitOfWorkFactory, T>();
        }

        public void WithUnitOfWorkFactory(IHandlerUnitOfWorkFactory unitOfWorkFactory)
        {
            _services.AddTransient<IHandlerUnitOfWorkFactory>(provider => unitOfWorkFactory);
        }

        public void WithUnitOfWorkFactory(Func<IServiceProvider, IHandlerUnitOfWorkFactory> implementationFactory)
        {
            _services.AddTransient<IHandlerUnitOfWorkFactory>(implementationFactory);
        }

        public void WithTopicSubscriberScopeFactory(ITopicSubscriberScopeFactory topicSubscriberScopeFactory)
        {
            _configurationBuilder.WithTopicSubscriberScopeFactory(topicSubscriberScopeFactory);
        }

        public void RegisterMessageHandler<TMessage, TMessageHandler>(string topic, string messageType)
            where TMessage : class, new()
            where TMessageHandler : class, IMessageHandler<TMessage>
        {
            _configurationBuilder.RegisterMessageHandler<TMessage, TMessageHandler>(topic, messageType);
            _services.AddTransient<TMessageHandler>();
        }

        public IConsumerConfiguration Build()
        {
            return _configurationBuilder.Build();
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

    public class ServiceProviderConsumerConfiguration : IConsumerConfiguration
    {
        private readonly IConsumerConfiguration _inner;
        private readonly IServiceProvider _provider;

        public ServiceProviderConsumerConfiguration(IConsumerConfiguration inner, IServiceProvider provider)
        {
            _inner = inner;
            _provider = provider;
        }

        public IMessageHandlerRegistry MessageHandlerRegistry => _inner.MessageHandlerRegistry;
        public IHandlerUnitOfWorkFactory UnitOfWorkFactory => _provider.GetRequiredService<IHandlerUnitOfWorkFactory>();
        public ITopicSubscriberScopeFactory TopicSubscriberScopeFactory => _inner.TopicSubscriberScopeFactory;
        public bool EnableAutoCommit => _inner.EnableAutoCommit;
        public IEnumerable<string> SubscribedTopics => _inner.SubscribedTopics;

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _inner).GetEnumerator();
        }
    }

    public class ServiceProviderUnitOfWorkFactory : IHandlerUnitOfWorkFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderUnitOfWorkFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IHandlerUnitOfWork CreateForHandlerType(Type handlerType)
        {
            return CreateUnitOfWork(_serviceProvider, handlerType);
        }

        protected virtual IHandlerUnitOfWork CreateUnitOfWork(IServiceProvider serviceProvider, Type handlerType)
        {
            return new ServiceProviderUnitOfWork(serviceProvider, handlerType);
        }
    }

    public class ServiceProviderUnitOfWork : IHandlerUnitOfWork
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Type _handlerType;

        public ServiceProviderUnitOfWork(IServiceProvider serviceProvider, Type handlerType)
        {
            _serviceProvider = serviceProvider;
            _handlerType = handlerType;
        }

        public Task Run(Func<object, Task> handlingAction)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                return RunInScope(scope, handlingAction);
            }
        }

        protected virtual Task RunInScope(IServiceScope scope, Func<object, Task> handlingAction)
        {
            var handler = scope.ServiceProvider.GetRequiredService(_handlerType);
            return handlingAction(handler);
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static void AddConsumer(this IServiceCollection services, Action<IConsumerConfigurationBuilder> options = null)
        {
            var configurationBuilder = new ServiceConsumerConfigurationBuilder(services);
            configurationBuilder.WithUnitOfWorkFactory<ServiceProviderUnitOfWorkFactory>();
            options?.Invoke(configurationBuilder);
            var configuration = configurationBuilder.Build();

            services.AddSingleton<IConsumerConfiguration>(provider => new ServiceProviderConsumerConfiguration(configuration, provider));
            services.AddTransient<Consumer>(provider => new Consumer(provider.GetRequiredService<IConsumerConfiguration>()));
            services.AddHostedService<SubscriberHostedService>();
        }
    }
}