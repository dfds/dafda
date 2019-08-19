using System;
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
            var configurationBuilder = new ConsumerConfigurationBuilder();
            options?.Invoke(configurationBuilder);
            IConsumerConfiguration configuration = configurationBuilder.Build();

            services.AddSingleton<IConfiguration>(configuration);

            services.AddTransient<ILocalMessageDispatcher>(provider => new LocalMessageDispatcher(configuration.MessageHandlerRegistry, new ServiceProviderBasedTypeResolver(provider)));

//            services.AddSingleton<ITopicProvider>(configuration.MessageHandlerRegistry);
//            services.AddSingleton<IMessageHandlerRegistry>(configuration.MessageHandlerRegistry);
//            services.AddTransient<IConsumerFactory, ConsumerFactory>();
//            services.AddTransient<ITypeResolver, ServiceProviderBasedTypeResolver>();
//            services.AddTransient<TopicSubscriber>();
//            services.AddHostedService<SubscriberHostedService>();
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