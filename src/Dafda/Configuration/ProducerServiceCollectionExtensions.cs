using System;
using System.Linq;
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
            EnsureServiceNotAlreadyRegistered<TService>(services);

            var producerOptions = new ProducerOptions();
            options?.Invoke(producerOptions);
            var producerConfiguration = producerOptions.Builder.Build();
            var outgoingMessageRegistry = producerOptions.OutgoingMessageRegistry;
            var producerProvider = new ProducerProvider<TImplementation>(producerConfiguration, outgoingMessageRegistry);
            
            services.AddTransient<TService, TImplementation>(provider =>
            {
                var producer = producerProvider.CreateProducer(provider);
                return ActivatorUtilities.CreateInstance<TImplementation>(provider, producer);
            });
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
            AddProducerFor<TClient, TClient>(services, options);
        }

        /// <summary>
        /// Add a Kafka producer available through the Microsoft dependency injection's <see cref="IServiceProvider"/>
        /// as <see cref="Producer"/>.
        ///
        /// Use this overload when configuration depends on other services (for example, IConfiguration).
        ///
        /// NOTE: currently only a single producer can be configured per <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> used in <c>Startup</c>.</param>
        /// <param name="optionsFactory">Factory that creates and configures <see cref="ProducerOptions"/> using the built <see cref="IServiceProvider"/>.</param>
        public static void AddProducerFor<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, ProducerOptions> optionsFactory)
            where TImplementation : class, TService
            where TService : class
        {
            EnsureServiceNotAlreadyRegistered<TService>(services);

            services.AddSingleton(provider =>
            {
                var producerOptions = optionsFactory(provider);
                var producerConfiguration = producerOptions.Builder.Build();
                var outgoingMessageRegistry = producerOptions.OutgoingMessageRegistry;
                return new ProducerProvider<TImplementation>(producerConfiguration, outgoingMessageRegistry);
            });
            
            services.AddTransient<TService, TImplementation>(provider =>
            {
                var producerProvider = provider.GetRequiredService<ProducerProvider<TImplementation>>();
                var producer = producerProvider.CreateProducer(provider);
                return ActivatorUtilities.CreateInstance<TImplementation>(provider, producer);
            });
        }

        /// <summary>
        /// Add a Kafka producer available through the Microsoft dependency injection's <see cref="IServiceProvider"/>
        /// as <see cref="Producer"/>.
        ///
        /// Use this overload when configuration depends on other services (for example, IConfiguration).
        ///
        /// NOTE: currently only a single producer can be configured per <typeparamref name="TClient"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> used in <c>Startup</c>.</param>
        /// <param name="optionsFactory">Factory that creates and configures <see cref="ProducerOptions"/> using the built <see cref="IServiceProvider"/>.</param>
        public static void AddProducerFor<TClient>(this IServiceCollection services, Func<IServiceProvider, ProducerOptions> optionsFactory) where TClient : class
        {
            AddProducerFor<TClient, TClient>(services, optionsFactory);
        }
    
        private static void EnsureServiceNotAlreadyRegistered<TService>(IServiceCollection services) where TService : class
        {
            if (services.Any(d => d.ServiceType == typeof(TService)))
            {
                throw new ProducerFactoryException(
                    $"A producer with the type \"{typeof(TService).FullName}\" has already been registered." +
                    $" Each producer should be registered with a unique service type.");
            }
        }
    }
    
    internal class ProducerProvider<TImplementation>(
        ProducerConfiguration configuration,
        OutgoingMessageRegistry messageRegistry)
        : IDisposable
    {
        private KafkaProducer _kafkaProducer;

        public Producer CreateProducer(IServiceProvider provider)
        {
            _kafkaProducer ??= configuration.KafkaProducerFactory(provider);

            var producer = new Producer(
                kafkaProducer: _kafkaProducer,
                outgoingMessageRegistry: messageRegistry,
                messageIdGenerator: configuration.MessageIdGenerator
            )
            {
                Name = typeof(TImplementation).FullName
            };

            return producer;
        }

        public void Dispose()
        {
            _kafkaProducer?.Dispose(); // to do why? it's shared across all producer instances created by this provider, so we can't dispose it after each producer is created. Instead, we dispose it when the provider itself is disposed, which happens when the application shuts down.
        }
    }
}