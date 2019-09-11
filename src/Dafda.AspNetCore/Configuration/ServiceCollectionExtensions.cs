using System;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public static class ConsumerConfigurationBuilderExtensions
    {
        public static ConsumerConfigurationBuilder WithConfigurationSource(this ConsumerConfigurationBuilder builder, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            builder.WithConfigurationSource(new DefaultConfigurationSource(configuration));

            return builder;
        }

        private class DefaultConfigurationSource : ConfigurationSource
        {
            private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

            public DefaultConfigurationSource(Microsoft.Extensions.Configuration.IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public override string GetByKey(string keyName)
            {
                return _configuration[keyName];
            }
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static void AddConsumer(this IServiceCollection services, Action<ConsumerConfigurationBuilder> options = null)
        {
            services.AddSingleton(provider =>
            {
                var configurationBuilder = new ConsumerConfigurationBuilder();
                configurationBuilder.WithUnitOfWorkFactory(new UnitOfWorkFactory(provider));

                options?.Invoke(configurationBuilder);

                return configurationBuilder.Build();
            });

            services.AddTransient<Consumer>();
            services.AddHostedService<SubscriberHostedService>();
        }

        private class UnitOfWorkFactory : IHandlerUnitOfWorkFactory
        {
            private readonly IServiceProvider _serviceProvider;

            public UnitOfWorkFactory(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public IHandlerUnitOfWork CreateForHandlerType(Type handlerType)
            {
                return new UnitOfWork(_serviceProvider, handlerType);
            }
        }

        private class UnitOfWork : IHandlerUnitOfWork
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly Type _handlerType;

            public UnitOfWork(IServiceProvider serviceProvider, Type handlerType)
            {
                _serviceProvider = serviceProvider;
                _handlerType = handlerType;
            }

            public Task Run(Func<object, Task> handlingAction)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService(_handlerType);

                    return handlingAction(service);
                }
            }
        }
    }
}