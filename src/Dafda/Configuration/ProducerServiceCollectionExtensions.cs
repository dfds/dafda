using System;
using System.Linq;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
    public static class ProducerServiceCollectionExtensions
    {
        public static void AddProducerFor<TClient>(this IServiceCollection services, Action<ProducerOptions> options) where TClient : class
        {
            var outgoingMessageRegistry = new OutgoingMessageRegistry();
            var configurationBuilder = new ProducerConfigurationBuilder();
            var consumerOptions = new ProducerOptions(configurationBuilder, outgoingMessageRegistry);
            options?.Invoke(consumerOptions);
            
            var producerConfiguration = configurationBuilder.Build();

            var factory = AddOrGetRegisteredProducerFactory(services);
            factory.ConfigureProducerFor<TClient>(producerConfiguration, outgoingMessageRegistry);

            services.AddTransient<TClient>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var producer = factory.GetFor<TClient>(loggerFactory);
                return ActivatorUtilities.CreateInstance<TClient>(provider, producer);
            });
        }

        private static ProducerFactory AddOrGetRegisteredProducerFactory(IServiceCollection services)
        {
            var factory = services
                .Where(x => x.ServiceType == typeof(ProducerFactory))
                .Select(x => x.ImplementationInstance)
                .Cast<ProducerFactory>()
                .SingleOrDefault();

            if (factory == null)
            {
                factory = new ProducerFactory();
                services.AddSingleton(factory);
            }

            return factory;
        }
    }
}