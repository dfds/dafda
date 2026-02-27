using System;
using System.Collections.Generic;
using System.Linq;
using Dafda.Consuming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration;

/// <summary></summary>
public static class ConsumerServiceCollectionExtensions
{
    private class ConsumerGroupIdRepository
    {
        private readonly ISet<string> _ids = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        public void Add(string newId) => _ids.Add(newId);
        public bool Contains(string id) => _ids.Contains(id);
    }

    /// <summary>
    /// Add a Kafka consumer. The consumer will run in an <see cref="IHostedService"/>.
    /// It is possible to configure multi consumers.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> used in <c>Startup</c>.</param>
    /// <param name="options">Use this action to override Dafda and underlying Kafka configuration.</param>
    public static void AddConsumer(this IServiceCollection services, Action<ConsumerOptions> options = null)
    {
        var consumerOptions = new ConsumerOptions();
        options?.Invoke(consumerOptions);
        var configuration = consumerOptions.Builder.Build();

        AddConsumerGroupIdRepositoryIfNeeded(services);
        EnsureGroupIdIsNotDuplicate(services, configuration.GroupId);

        services.AddSingleton<IHostedService, ConsumerHostedService>(provider => new ConsumerHostedService(
            logger: provider.GetRequiredService<ILogger<ConsumerHostedService>>(),
            applicationLifetime: provider.GetRequiredService<IHostApplicationLifetime>(),
            consumer: new Consumer(
                configuration.MessageHandlerRegistry,
                configuration.UnitOfWorkFactory(provider),
                configuration.ConsumerScopeFactory(provider),
                configuration.UnconfiguredMessageHandlingStrategy(provider),
                configuration.MessageFilter,
                configuration.MessageHandlerExecutionStrategyFactory(provider),
                configuration.EnableAutoCommit
            ),
            configuration.GroupId,
            configuration.ConsumerErrorHandler
        ));
    }
        
    /// <summary>
    /// Add a Kafka consumer. The consumer will run in an <see cref="IHostedService"/>.
    /// It is possible to configure multi consumers.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> used in <c>Startup</c>.</param>
    /// <param name="consumerOptionsFactory">Use this action to override Dafda and underlying Kafka configuration.</param>
    public static void AddConsumer(this IServiceCollection services, Func<IServiceProvider, ConsumerOptions> consumerOptionsFactory)
    {
        AddConsumerGroupIdRepositoryIfNeeded(services);
            
        services.AddSingleton<IHostedService, ConsumerHostedService>(provider =>
        {
            var consumerOptions2 = consumerOptionsFactory.Invoke(provider);
            var configuration = consumerOptions2.Builder.Build();
                
            EnsureGroupIdIsNotDuplicate(services, configuration.GroupId);
                
            return new ConsumerHostedService(
                logger: provider.GetRequiredService<ILogger<ConsumerHostedService>>(),
                applicationLifetime: provider.GetRequiredService<IHostApplicationLifetime>(),
                consumer: new Consumer(
                    configuration.MessageHandlerRegistry,
                    configuration.UnitOfWorkFactory(provider),
                    configuration.ConsumerScopeFactory(provider),
                    configuration.UnconfiguredMessageHandlingStrategy(provider),
                    configuration.MessageFilter,
                    configuration.MessageHandlerExecutionStrategyFactory(provider),
                    configuration.EnableAutoCommit
                ),
                configuration.GroupId,
                configuration.ConsumerErrorHandler
            );
        });
    }

    private static void AddConsumerGroupIdRepositoryIfNeeded(IServiceCollection services)
    {
        var consumerGroupIdRepository = services
            .Where(x => x.ServiceType == typeof(ConsumerGroupIdRepository))
            .Where(x => x.Lifetime == ServiceLifetime.Singleton)
            .Where(x => x.ImplementationInstance != null)
            .Where(x => x.ImplementationInstance.GetType() == typeof(ConsumerGroupIdRepository))
            .Select(x => x.ImplementationInstance)
            .Cast<ConsumerGroupIdRepository>()
            .FirstOrDefault();

        if (consumerGroupIdRepository != null) return;
            
        consumerGroupIdRepository = new ConsumerGroupIdRepository();
        services.AddSingleton(consumerGroupIdRepository);
    }
        
    private static void EnsureGroupIdIsNotDuplicate(IServiceCollection services, string groupId)
    {
        var consumerGroupIdRepository = services
            .Where(x => x.ServiceType == typeof(ConsumerGroupIdRepository))
            .Where(x => x.Lifetime == ServiceLifetime.Singleton)
            .Where(x => x.ImplementationInstance != null)
            .Where(x => x.ImplementationInstance.GetType() == typeof(ConsumerGroupIdRepository))
            .Select(x => x.ImplementationInstance)
            .Cast<ConsumerGroupIdRepository>()
            .Single();

        if (consumerGroupIdRepository.Contains(groupId))
        {
            throw new InvalidConfigurationException($"Multiple consumers CANNOT be configured with same consumer group id \"{groupId}\".");
        }

        consumerGroupIdRepository.Add(groupId);
    }
}