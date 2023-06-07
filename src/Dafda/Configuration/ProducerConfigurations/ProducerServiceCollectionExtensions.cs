using System;
using System.Linq;
using Dafda.Configuration.ProducerConfigurations;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
    /// <summary></summary>
    public static class ProducerServiceCollectionExtensions
    {
        /// <summary>
        /// Add a Kafka producer available through the Microsoft dependency injection's <see cref="IServiceProvider"/>
        /// as <see cref="Producer"/>. 
        ///
        /// NOTE: currently only a single producer can be configured per <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> used in <c>Startup</c>.</param>
        /// <param name="options">Use this action to override Dafda and underlying Kafka configuration.</param>
        public static void AddProducerFor<TService, TImplementation>(this IServiceCollection services, Action<ProducerOptions> options)
            where TImplementation : class, TService
            where TService : class
        {
            var factory = CreateProducerFactory<TImplementation>(services, options);
            services.AddTransient<TService, TImplementation>(provider =>
                CreateInstance<TImplementation>(provider, factory));
        }

        /// <summary>
        /// Add a Kafka producer available through the Microsoft dependency injection's <see cref="IServiceProvider"/>
        /// as <see cref="Producer"/>. 
        ///
        /// NOTE: currently only a single producer can be configured per <typeparamref name="TClient"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> used in <c>Startup</c>.</param>
        /// <param name="options">Use this action to override Dafda and underlying Kafka configuration.</param>
        public static void AddProducerFor<TClient>(this IServiceCollection services, Action<ProducerOptions> options) where TClient : class
        {
            var factory = CreateProducerFactory<TClient>(services, options);
            services.AddTransient(provider =>
                 CreateInstance<TClient>(provider, factory));
        }

        private static TImplementation CreateInstance<TImplementation>(IServiceProvider provider, ProducerFactory factory)
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var producer = factory.GetFor<TImplementation>(loggerFactory);
            return ActivatorUtilities.CreateInstance<TImplementation>(provider, producer);
        }

        private static ProducerFactory CreateProducerFactory<TImplementation>(IServiceCollection services, Action<ProducerOptions> options)
        {
            var outgoingMessageRegistry = new OutgoingMessageRegistry();
            var configurationBuilder = new ProducerConfigurationBuilder();
            var consumerOptions = new ProducerOptions(configurationBuilder, outgoingMessageRegistry);
            options?.Invoke(consumerOptions);

            var producerConfiguration = configurationBuilder.Build();

            var factory = AddOrGetRegisteredProducerFactory(services);
            factory.ConfigureProducerFor<TImplementation>(producerConfiguration, outgoingMessageRegistry);
            return factory;
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