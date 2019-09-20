using System;
using System.Collections;
using System.Collections.Generic;
using Dafda.Consuming;
using Dafda.Messaging;
using Dafda.Producing;
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

        public static void AddProducer(this IServiceCollection services, Action<IProducerOptions> options)
        {
            var configurationBuilder = new ProducerConfigurationBuilder();
            var consumerOptions = new ProducerOptions(configurationBuilder);
            options?.Invoke(consumerOptions);
            var configuration = configurationBuilder.Build();

            var factory = new KafkaProducerFactory();
            var producer = factory.CreateProducer(configuration);
            var bus = new Bus(producer);

            services.AddSingleton<IBus>(bus);
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
        public ICommitStrategy CommitStrategy => _inner.CommitStrategy;
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