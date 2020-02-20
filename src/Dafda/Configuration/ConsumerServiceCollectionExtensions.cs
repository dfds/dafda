using System;
using System.Collections.Generic;
using System.Linq;
using Dafda.Consuming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
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

        public static void AddConsumer(this IServiceCollection services, Action<ConsumerOptions> options = null)
        {
            var configurationBuilder = new ConsumerConfigurationBuilder();
            var consumerOptions = new ConsumerOptions(configurationBuilder, services);
            consumerOptions.WithUnitOfWorkFactory<ServiceProviderUnitOfWorkFactory>();
            consumerOptions.WithUnitOfWork<ScopedUnitOfWork>();
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
            
            Func<IServiceProvider, SubscriberHostedService> hostedServiceFactory = provider => new SubscriberHostedService(
                logger: provider.GetRequiredService<ILogger<SubscriberHostedService>>(),
                applicationLifetime: provider.GetRequiredService<IApplicationLifetime>(),
                consumer: new Consumer(
                        messageHandlerRegistry: configuration.MessageHandlerRegistry,
                        unitOfWorkFactory: provider.GetRequiredService<IHandlerUnitOfWorkFactory>(),
                        consumerScopeFactory: configuration.ConsumerScopeFactory,
                        isAutoCommitEnabled: configuration.EnableAutoCommit
                    )
            );

            services.AddTransient<IHostedService, SubscriberHostedService>(hostedServiceFactory);
            services.AddTransient<SubscriberHostedService>(hostedServiceFactory);
        }
    }
}