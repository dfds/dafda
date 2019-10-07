using System;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public static class ProducerServiceCollectionExtensions
    {
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
}