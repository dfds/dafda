using System;
using System.Collections.Generic;
using System.Linq;
using Dafda.Consuming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
    /// <summary></summary>
    public static class ConsumerServiceCollectionExtensions
    {
        private class ConsumerGroupIdRepository
        {
            private readonly ISet<string> _ids;

            public ConsumerGroupIdRepository()
            {
                _ids = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            }

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
            var configurationBuilder = new ConsumerConfigurationBuilder();
            var consumerOptions = new ConsumerOptions(configurationBuilder, services);
            consumerOptions.WithUnitOfWorkFactory<ServiceProviderUnitOfWorkFactory>();
            consumerOptions.WithUnconfiguredMessageHandlingStrategy<RequireExplicitHandlers>();
            options?.Invoke(consumerOptions);
            var configuration = configurationBuilder.Build();

            var consumerGroupIdRepository = services
                .Where(x => x.ServiceType == typeof(ConsumerGroupIdRepository))
                .Where(x => x.Lifetime == ServiceLifetime.Singleton)
                .Where(x => x.ImplementationInstance != null)
                .Where(x => x.ImplementationInstance.GetType() == typeof(ConsumerGroupIdRepository))
                .Select(x => x.ImplementationInstance)
                .Cast<ConsumerGroupIdRepository>()
                .FirstOrDefault();

            if (consumerGroupIdRepository == null)
            {
                consumerGroupIdRepository = new ConsumerGroupIdRepository();
                services.AddSingleton(consumerGroupIdRepository);
            }

            if (consumerGroupIdRepository.Contains(configuration.GroupId))
            {
                throw new InvalidConfigurationException($"Multiple consumers CANNOT be configured with same consumer group id \"{configuration.GroupId}\".");
            }

            consumerGroupIdRepository.Add(configuration.GroupId);

            ConsumerHostedService HostedServiceFactory(IServiceProvider provider) => new ConsumerHostedService(
                logger: provider.GetRequiredService<ILogger<ConsumerHostedService>>(),
                applicationLifetime: provider.GetRequiredService<IHostApplicationLifetime>(),
                consumer: new Consumer(
                    configuration.MessageHandlerRegistry,
                    provider.GetRequiredService<IHandlerUnitOfWorkFactory>(),
                    configuration.ConsumerScopeFactory(provider),
                    provider.GetRequiredService<IUnconfiguredMessageHandlingStrategy>(),
                    configuration.MessageFilter,
                    configuration.EnableAutoCommit
                ),
                configuration.GroupId
            );

            services.AddTransient<IHostedService, ConsumerHostedService>(HostedServiceFactory);
            services.AddTransient<ConsumerHostedService>(HostedServiceFactory); // NOTE: [jandr] is this needed?
        }
    }
}
