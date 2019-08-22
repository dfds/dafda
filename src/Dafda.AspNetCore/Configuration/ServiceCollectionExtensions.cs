using System;
using Dafda.Consuming;
using Dafda.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IMessageHandlerRegistrationBuilder AddConsumer(this IServiceCollection services, Action<ConsumerConfigurationBuilder> options = null)
        {
            services.AddSingleton(provider =>
            {
//                var logger = provider.GetService<ILogger<ConsumerConfiguration>>();

                var defaultConfiguration = provider.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
                var configurationProvider = new DefaultConfigurationSource(defaultConfiguration);

                var configurationBuilder = new ConsumerConfigurationBuilder();
                configurationBuilder.UseConfigurationSource(configurationProvider);

                options?.Invoke(configurationBuilder);

                var configuration = configurationBuilder.Build();

                foreach (var (key, value) in configuration)
                {
//                    logger.LogDebug("CFG: {Key}= {Value}", key, value);
                }

                return configuration;
            });

            var handlerRegistry = new MessageHandlerRegistry();

            services.AddSingleton<ITopicProvider>(handlerRegistry);
            services.AddSingleton<IMessageHandlerRegistry>(handlerRegistry);
            services.AddTransient<IConsumerFactory, ConsumerFactory>();
            services.AddTransient<ILocalMessageDispatcher, LocalMessageDispatcher>();
            services.AddTransient<ITypeResolver, ServiceProviderBasedTypeResolver>();
            services.AddTransient<TopicSubscriber>();
            services.AddHostedService<SubscriberHostedService>();

            return new MessageHandlerRegistrationBuilder(services, handlerRegistry);
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