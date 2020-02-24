using System;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public static class ProducerServiceCollectionExtensions
    {
        public static void AddProducer(this IServiceCollection services, Action<ProducerOptions> options)
        {
            var outgoingMessageRegistry = new OutgoingMessageRegistry();
            var configurationBuilder = new ProducerConfigurationBuilder();
            var consumerOptions = new ProducerOptions(configurationBuilder, outgoingMessageRegistry);
            options?.Invoke(consumerOptions);
            
            var producerConfiguration = configurationBuilder.Build();

            services.AddSingleton<IProducer>(provider =>
            {
                var messageIdGenerator = producerConfiguration.MessageIdGenerator;
                var kafkaProducer = producerConfiguration.KafkaProducerFactory();

                return new Producer(kafkaProducer, outgoingMessageRegistry, messageIdGenerator);
            });
        }
    }
}