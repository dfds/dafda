using System;
using System.Linq;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;

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
            EnsureServiceIsNotAlreadyRegistered<TImplementation>(services);

            services.AddSingleton(_ =>
            {
                var producerOptions = new ProducerOptions();
                options?.Invoke(producerOptions);
                return new ProducerProvider<TImplementation>(producerOptions);
            });
            
            services.AddTransient<TService, TImplementation>(CreateImplementation<TService, TImplementation>);
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
            EnsureServiceIsNotAlreadyRegistered<TImplementation>(services);

            services.AddSingleton(provider =>
            {
                var producerOptions = optionsFactory(provider);
                return new ProducerProvider<TImplementation>(producerOptions);
            });
            
            services.AddTransient<TService, TImplementation>(CreateImplementation<TService, TImplementation>);
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
    
        private static void EnsureServiceIsNotAlreadyRegistered<TService>(IServiceCollection services) where TService : class
        {
            if (services.Any(d => d.ServiceType == typeof(TService)))
            {
                throw new ProducerFactoryException(
                    $"A producer with the type \"{typeof(TService).FullName}\" has already been registered." +
                    $" Each producer should be registered with a unique service type.");
            }
        }

        private static TImplementation CreateImplementation<TService, TImplementation>(IServiceProvider provider)
            where TImplementation : class, TService
            where TService : class
        {
            var producerProvider = provider.GetRequiredService<ProducerProvider<TImplementation>>();
            var producer = producerProvider.CreateProducer(provider);
            return ActivatorUtilities.CreateInstance<TImplementation>(provider, producer);
        }
    }
    
    internal class ProducerProvider<TImplementation> : IDisposable
    {
        private readonly ProducerConfiguration _configuration;
        private readonly OutgoingMessageRegistry _messageRegistry;
        private KafkaProducer _kafkaProducer;

        public ProducerProvider(ProducerOptions options)
        {
            _configuration = options.Builder.Build();
            _messageRegistry = options.OutgoingMessageRegistry;
        }

        public Producer CreateProducer(IServiceProvider provider)
        {
            _kafkaProducer ??= _configuration.KafkaProducerFactory(provider);

            var producer = new Producer(
                kafkaProducer: _kafkaProducer,
                outgoingMessageRegistry: _messageRegistry,
                messageIdGenerator: _configuration.MessageIdGenerator
            )
            {
                Name = typeof(TImplementation).FullName
            };

            return producer;
        }

        public void Dispose()
        {
            _kafkaProducer?.Dispose();
        }
    }
}