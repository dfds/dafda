using System;
using System.Collections;
using System.Collections.Generic;
using Dafda.Consuming;
using Dafda.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static void AddConsumer(this IServiceCollection services, Action<IConsumerOptions> options = null)
        {
            var configurationBuilder = new ConsumerConfigurationBuilder();
            var consumerOptions = new ConsumerOptions(configurationBuilder, services);
            consumerOptions.WithUnitOfWorkFactory<ServiceProviderUnitOfWorkFactory>();
            consumerOptions.WithUnitOfWork<ScopedUnitOfWork>();
            options?.Invoke(consumerOptions);
            var configuration = configurationBuilder.Build();

            services.AddSingleton<IConsumerConfiguration>(provider => new ServiceProviderConsumerConfiguration(configuration, provider));
            services.AddTransient(provider => new Consumer(provider.GetRequiredService<IConsumerConfiguration>()));
            services.AddHostedService<SubscriberHostedService>();
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
}