using System;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public static class ProducerServiceCollectionExtensions
    {
        public static void AddProducer(this IServiceCollection services, Action<IProducerOptions> options)
        {
            var producerConfiguration = ConfigureProducerConfiguration(services, options);

            services.AddSingleton(producerConfiguration);
            services.AddSingleton<IProducer>(provider =>
            {
                var configuration = provider.GetRequiredService<IProducerConfiguration>();
                var kafkaProducer = configuration.KafkaProducerFactory.CreateProducer(configuration);
                return new Producer(kafkaProducer, producerConfiguration.OutgoingMessageRegistry, producerConfiguration.MessageIdGenerator);
            });
        }

        private static IProducerConfiguration ConfigureProducerConfiguration(IServiceCollection services, Action<IProducerOptions> options)
        {
            var outgoingMessageRegistry = new OutgoingMessageRegistry();
            var configurationBuilder = new ProducerConfigurationBuilder();
            configurationBuilder.WithOutgoingMessageRegistry(outgoingMessageRegistry);
            var consumerOptions = new ProducerOptions(configurationBuilder, services, outgoingMessageRegistry);
            options?.Invoke(consumerOptions);
            var producerConfiguration = configurationBuilder.Build();
            return producerConfiguration;
        }
    }
}