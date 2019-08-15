using System;
using Dafda.Consuming;
using Dafda.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureConsumer(this IServiceCollection services, Action<IConsumerConfiguration> config)
        {
            var handlerRegistry = new MessageHandlerRegistry();
            var handlerConfiguration = new MessageHandlerConfiguration(services, handlerRegistry);
            var consumerConfiguration = new ConsumerConfiguration(handlerConfiguration);
            config(consumerConfiguration);

            services.AddSingleton(handlerRegistry);
            services.AddSingleton<IMessageHandlerRegistry>(handlerRegistry);

            services.AddSingleton(provider =>
            {
                var logger = provider.GetService<ILogger<ConsumerConfiguration>>();
                var configurationProvider = new DefaultConfigurationProvider(provider.GetService<Microsoft.Extensions.Configuration.IConfiguration>());

                var configuration = consumerConfiguration.BuildConfiguration(configurationProvider);

                foreach (var (key, value) in configuration)
                {
                    logger.LogDebug("CFG: {Key}= {Value}", key, value);
                }

                return configuration;
            });

            services.AddTransient<IConsumerFactory, ConsumerFactory>();
            services.AddTransient<ILocalMessageDispatcher, LocalMessageDispatcher>();
            services.AddTransient<ITypeResolver, ServiceProviderBasedTypeResolver>();
            services.AddTransient<TopicSubscriber>();
            services.AddHostedService<SubscriberHostedService>();

            return services;
        }

        private class DefaultConfigurationProvider : IConfigurationProvider
        {
            private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

            public DefaultConfigurationProvider(Microsoft.Extensions.Configuration.IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public string GetByKey(string keyName)
            {
                return _configuration[keyName];
            }
        }

        private class ServiceProviderBasedTypeResolver : ITypeResolver
        {
            private readonly IServiceProvider _serviceProvider;

            public ServiceProviderBasedTypeResolver(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public object Resolve(Type instanceType)
            {
                return _serviceProvider.GetService(instanceType);
            }
        }
    }
}