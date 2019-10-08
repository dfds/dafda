using System;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public static class ProducerServiceCollectionExtensions
    {
        public static void AddProducer(this IServiceCollection services, Action<IProducerOptions> options)
        {
            var outgoingMessageRegistry = new OutgoingMessageRegistry();
            var configurationBuilder = new ProducerConfigurationBuilder();
            configurationBuilder.WithOutgoingMessageRegistry(outgoingMessageRegistry);
            var consumerOptions = new ProducerOptions(configurationBuilder, outgoingMessageRegistry);
            options?.Invoke(consumerOptions);
            var configuration = configurationBuilder.Build();
            
            var kafkaProducer = configuration.KafkaProducerFactory.CreateProducer(configuration);
            var producer = new Producer(kafkaProducer, configuration.OutgoingMessageRegistry, configuration.MessageIdGenerator);

            services.AddSingleton<IProducer>(producer);
        }
    }
}