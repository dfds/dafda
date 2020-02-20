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
            var consumerOptions = new ProducerOptions(configurationBuilder, services, outgoingMessageRegistry);
            options?.Invoke(consumerOptions);
            var producerConfiguration = configurationBuilder.Build();
            var configuration = producerConfiguration;

            services.AddSingleton<IProducer>(provider =>
            {
                var kafkaProducerFactory = configuration.KafkaProducerFactory;
                IKafkaProducer kafkaProducer = kafkaProducerFactory.CreateProducer();
                var messageIdGenerator = configuration.MessageIdGenerator;
                return new Producer(kafkaProducer, outgoingMessageRegistry, messageIdGenerator);
            });
        }
    }
}