using System;
using System.Collections.Generic;
using Dafda.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public interface IConsumerConfiguration
    {
        IMessageHandlerConfiguration Handlers { get; }

        IConsumerConfiguration FromEnvironmentWithPrefix(string prefix, params string[] alternativePrefixes);
        IConsumerConfiguration WithConfiguration(string key, string value);
        IConsumerConfiguration WithGroupId(string groupId);
        IConsumerConfiguration WithBootstrapServers(string bootstrapServers);
        IConsumerConfiguration WithBootstrapServers(params string[] bootstrapServers);
        IConsumerConfiguration WithEnabledAutoCommit(bool enableAutoCommit);
    }

    internal class ConsumerConfiguration : IConsumerConfiguration
    {
        private readonly IList<Action<ConsumerConfigurationBuilder, ConfigurationProvider>> _builders = new List<Action<ConsumerConfigurationBuilder, ConfigurationProvider>>();
        private readonly MessageHandlerConfiguration _messageHandlerConfiguration;

        public ConsumerConfiguration(MessageHandlerConfiguration messageHandlerConfiguration)
        {
            _messageHandlerConfiguration = messageHandlerConfiguration;
        }

        public IMessageHandlerConfiguration Handlers => _messageHandlerConfiguration;

        public IConsumerConfiguration ConfigureHandlers(Action<IMessageHandlerConfiguration> config)
        {
            config?.Invoke(_messageHandlerConfiguration);

            return this;
        }

        public IConsumerConfiguration WithGroupId(string groupId)
        {
            return WithConfiguration(ConfigurationKey.GroupId, groupId);
        }

        public IConsumerConfiguration WithBootstrapServers(string bootstrapServers)
        {
            return WithConfiguration(ConfigurationKey.BootstrapServers, bootstrapServers);
        }

        public IConsumerConfiguration WithBootstrapServers(params string[] bootstrapServers)
        {
            return WithConfiguration(ConfigurationKey.BootstrapServers, string.Join(",", bootstrapServers));
        }

        public IConsumerConfiguration WithEnabledAutoCommit(bool enableAutoCommit)
        {
            return WithConfiguration(ConfigurationKey.EnableAutoCommit, enableAutoCommit ? "true" : "false");
        }

        public IConsumerConfiguration FromEnvironmentWithPrefix(string prefix, params string[] alternativePrefixes)
        {
            _builders.Add((builder, provider) =>
            {
                builder.WithConfigurationProvider(provider);
                builder.WithNamingConvention(NamingConvention.UseEnvironmentStyle(prefix));
                foreach (var alternativePrefix in alternativePrefixes)
                {
                    builder.WithNamingConvention(NamingConvention.UseEnvironmentStyle(alternativePrefix));
                }
            });

            return this;
        }

        public IConsumerConfiguration WithConfiguration(string key, string value)
        {
            _builders.Add((builder, provider) => builder.WithConfiguration(key, value));
            return this;
        }

        public IConfiguration BuildConfiguration(ConfigurationProvider defaultConfigurationProvider)
        {
            var configurationBuilder = new ConsumerConfigurationBuilder();

            foreach (var builder in _builders)
            {
                builder(configurationBuilder, defaultConfigurationProvider);
            }

            return configurationBuilder.Build();
        }
    }
}