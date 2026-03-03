using System;
using System.Linq;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration;

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

        var factory = AddOrGetRegisteredProducerFactory(services);
        
        var producerOptions = new ProducerOptions();
        options?.Invoke(producerOptions);
        var producerConfiguration = producerOptions.Builder.Build();
        var outgoingMessageRegistry = producerOptions.OutgoingMessageRegistry;
        factory.ConfigureProducerFor<TImplementation>(producerConfiguration, outgoingMessageRegistry);

        services.AddTransient<TService, TImplementation>(provider => CreateInstance<TImplementation>(provider, factory));
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

        services.AddTransient<TService, TImplementation>(provider =>
        {
            var factory = AddOrGetRegisteredProducerFactory(services);

            if (!factory.IsConfigured<TImplementation>())
            {
                var producerOptions = optionsFactory(provider);
                var producerConfiguration = producerOptions.Builder.Build();
                var outgoingMessageRegistry = producerOptions.OutgoingMessageRegistry;
                factory.ConfigureProducerFor<TImplementation>(producerConfiguration, outgoingMessageRegistry);
            }

            return CreateInstance<TImplementation>(provider, factory);
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
        EnsureServiceNotAlreadyRegistered<TClient>(services);

        var factory = AddOrGetRegisteredProducerFactory(services);
        
        var producerOptions = new ProducerOptions();
        options?.Invoke(producerOptions);
        var producerConfiguration = producerOptions.Builder.Build();
        var outgoingMessageRegistry = producerOptions.OutgoingMessageRegistry;
        factory.ConfigureProducerFor<TClient>(producerConfiguration, outgoingMessageRegistry);

        services.AddTransient(provider => CreateInstance<TClient>(provider, factory));
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
        EnsureServiceNotAlreadyRegistered<TClient>(services);

        services.AddTransient<TClient>(provider =>
        {
            var factory = AddOrGetRegisteredProducerFactory(services);

            if (!factory.IsConfigured<TClient>())
            {
                var producerOptions = optionsFactory(provider);
                var producerConfiguration = producerOptions.Builder.Build();
                var outgoingMessageRegistry = producerOptions.OutgoingMessageRegistry;
                factory.ConfigureProducerFor<TClient>(producerConfiguration, outgoingMessageRegistry);
            }

            return CreateInstance<TClient>(provider, factory);
        });
    }
    
    private static void EnsureServiceNotAlreadyRegistered<TService>(IServiceCollection services) where TService : class
    {
        if (services.Any(d => d.ServiceType == typeof(TService)))
        {
            throw new ProducerFactoryException(
                $"A service registration for '{typeof(TService).FullName}' already exists. " +
                "Dafda producers can only be registered once per service type. " +
                "Remove the existing registration or use a different service type.");
        }
    }

    private static TImplementation CreateInstance<TImplementation>(IServiceProvider provider, ProducerFactory factory)
    {
        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
        var producer = factory.GetFor<TImplementation>(loggerFactory);
        return ActivatorUtilities.CreateInstance<TImplementation>(provider, producer);
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