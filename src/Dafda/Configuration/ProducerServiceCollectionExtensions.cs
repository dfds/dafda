using System;
using System.Linq;
using System.Threading;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    /// <summary>
    /// Extension methods for registering Kafka producers with the Microsoft dependency injection container.
    /// </summary>
    public static class ProducerServiceCollectionExtensions
    {
        /// <summary>
        /// Add a Kafka producer available through the Microsoft dependency injection's <see cref="IServiceProvider"/>
        /// as <typeparamref name="TService"/>. Each <typeparamref name="TService"/> must be unique —
        /// duplicate registrations will throw a <see cref="ProducerFactoryException"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> used in <c>Startup</c>.</param>
        /// <param name="options">Use this action to override Dafda and underlying Kafka configuration.</param>
        public static void AddProducerFor<TService, TImplementation>(this IServiceCollection services, Action<ProducerOptions> options) 
            where TImplementation : class, TService 
            where TService : class
        {
            ThrowIfProducerServiceAlreadyRegisteredFor<TService>(services);

            services.AddSingleton(provider =>
            {
                var producerOptions = new ProducerOptions();
                options?.Invoke(producerOptions);
                return new ProducerFactory<TService>(provider, producerOptions);
            });
            
            services.AddTransient<TService, TImplementation>(CreateProducerService<TService, TImplementation>);
        }

        /// <summary>
        /// Add a Kafka producer available through the Microsoft dependency injection's <see cref="IServiceProvider"/>
        /// as <typeparamref name="TClient"/>. Each <typeparamref name="TClient"/> must be unique —
        /// duplicate registrations will throw a <see cref="ProducerFactoryException"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> used in <c>Startup</c>.</param>
        /// <param name="options">Use this action to override Dafda and underlying Kafka configuration.</param>
        public static void AddProducerFor<TClient>(this IServiceCollection services, Action<ProducerOptions> options) where TClient : class
        {
            AddProducerFor<TClient, TClient>(services, options);
        }

        /// <summary>
        /// Add a Kafka producer available through the Microsoft dependency injection's <see cref="IServiceProvider"/>
        /// as <typeparamref name="TService"/>. Each <typeparamref name="TService"/> must be unique —
        /// duplicate registrations will throw a <see cref="ProducerFactoryException"/>.
        ///
        /// Use this overload when configuration depends on other services (for example, <c>IConfiguration</c>).
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> used in <c>Startup</c>.</param>
        /// <param name="optionsFactory">Factory that creates and configures <see cref="ProducerOptions"/> using the built <see cref="IServiceProvider"/>.</param>
        public static void AddProducerFor<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, ProducerOptions> optionsFactory)
            where TImplementation : class, TService
            where TService : class
        {
            ThrowIfProducerServiceAlreadyRegisteredFor<TService>(services);

            services.AddSingleton(provider =>
            {
                var options = optionsFactory(provider);
                return new ProducerFactory<TService>(provider, options);
            });
            
            services.AddTransient<TService, TImplementation>(CreateProducerService<TService, TImplementation>);
        }

        /// <summary>
        /// Add a Kafka producer available through the Microsoft dependency injection's <see cref="IServiceProvider"/>
        /// as <typeparamref name="TClient"/>. Each <typeparamref name="TClient"/> must be unique —
        /// duplicate registrations will throw a <see cref="ProducerFactoryException"/>.
        ///
        /// Use this overload when configuration depends on other services (for example, <c>IConfiguration</c>).
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> used in <c>Startup</c>.</param>
        /// <param name="optionsFactory">Factory that creates and configures <see cref="ProducerOptions"/> using the built <see cref="IServiceProvider"/>.</param>
        public static void AddProducerFor<TClient>(this IServiceCollection services, Func<IServiceProvider, ProducerOptions> optionsFactory) where TClient : class
        {
            AddProducerFor<TClient, TClient>(services, optionsFactory);
        }
    
        private static void ThrowIfProducerServiceAlreadyRegisteredFor<TService>(IServiceCollection services) where TService : class
        {
            if (services.Any(d => d.ServiceType == typeof(TService)))
            {
                throw new ProducerFactoryException(
                    $"Service type \"{typeof(TService).FullName}\" is already registered in the dependency injection container. Each Dafda producer must use a unique service type.");
            }
        }

        private static TImplementation CreateProducerService<TService, TImplementation>(IServiceProvider provider)
            where TImplementation : class, TService
            where TService : class
        {
            var producerFactory = provider.GetRequiredService<ProducerFactory<TService>>();
            var producer = producerFactory.CreateProducerInstance();
            return ActivatorUtilities.CreateInstance<TImplementation>(provider, producer);
        }
    }
    
    internal class ProducerFactory<TService>(IServiceProvider provider, ProducerOptions options) : IDisposable
    {
        private readonly ProducerConfiguration _configuration = options.Builder.Build();
        private readonly OutgoingMessageRegistry _messageRegistry = options.OutgoingMessageRegistry;
        private Lazy<KafkaProducer> _kafkaProducer;

        public Producer CreateProducerInstance()
        {
            // Use LazyInitializer to ensure thread-safe, once-only construction while
            // capturing `provider` in the factory delegate.
            LazyInitializer.EnsureInitialized(
                ref _kafkaProducer,
                () => new Lazy<KafkaProducer>(() => _configuration.KafkaProducerFactory(provider), isThreadSafe: true));

            var producer = new Producer(
                kafkaProducer: _kafkaProducer.Value,
                outgoingMessageRegistry: _messageRegistry,
                messageIdGenerator: _configuration.MessageIdGenerator
            )
            {
                Name = typeof(TService).FullName
            };

            return producer;
        }

        public void Dispose()
        {
            if (_kafkaProducer is { IsValueCreated: true })
            {
                _kafkaProducer.Value.Dispose();
            }
        }
    }
}
