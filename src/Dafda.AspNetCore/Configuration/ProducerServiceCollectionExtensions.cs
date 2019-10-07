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

            var bus = new Bus(KafkaProducerFactory.CreateProducer(configuration), configuration);

            services.AddSingleton<IBus>(bus);
        }
    }
}