using System;
using System.Collections.Generic;
using System.Linq;
using Dafda.Consuming;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
    internal sealed class ConsumerConfigurationBuilder
    {
        private static readonly string[] DefaultConfigurationKeys =
        {
            ConfigurationKey.GroupId,
            ConfigurationKey.EnableAutoCommit,
            ConfigurationKey.BootstrapServers,
            ConfigurationKey.BrokerVersionFallback,
            ConfigurationKey.ApiVersionFallbackMs,
            ConfigurationKey.SslCaLocation,
            ConfigurationKey.SaslUsername,
            ConfigurationKey.SaslPassword,
            ConfigurationKey.SaslMechanisms,
            ConfigurationKey.SecurityProtocol,
        };

        private static readonly string[] RequiredConfigurationKeys =
        {
            ConfigurationKey.GroupId,
            ConfigurationKey.BootstrapServers
        };

        private readonly IDictionary<string, string> _configurations = new Dictionary<string, string>();
        private readonly IList<NamingConvention> _namingConventions = new List<NamingConvention>();
        private readonly MessageHandlerRegistry _messageHandlerRegistry = new MessageHandlerRegistry();

        private ConfigurationSource _configurationSource = ConfigurationSource.Null;
        private IHandlerUnitOfWorkFactory _unitOfWorkFactory;
        private Func<ILoggerFactory, IConsumerScopeFactory> _consumerScopeFactory;
        private IIncomingMessageFactory _incomingMessageFactory = new JsonIncomingMessageFactory();
        private bool _readFromBeginning;

        public ConsumerConfigurationBuilder WithConfigurationSource(ConfigurationSource configurationSource)
        {
            _configurationSource = configurationSource;
            return this;
        }

        public ConsumerConfigurationBuilder WithNamingConvention(Func<string, string> converter)
        {
            _namingConventions.Add(NamingConvention.UseCustom(converter));
            return this;
        }

        internal ConsumerConfigurationBuilder WithNamingConvention(NamingConvention namingConvention)
        {
            _namingConventions.Add(namingConvention);
            return this;
        }

        public ConsumerConfigurationBuilder WithEnvironmentStyle(string prefix = null, params string[] additionalPrefixes)
        {
            WithNamingConvention(NamingConvention.UseEnvironmentStyle(prefix));

            foreach (var additionalPrefix in additionalPrefixes)
            {
                WithNamingConvention(NamingConvention.UseEnvironmentStyle(additionalPrefix));
            }

            return this;
        }

        public ConsumerConfigurationBuilder WithConfiguration(string key, string value)
        {
            _configurations[key] = value;
            return this;
        }

        public ConsumerConfigurationBuilder WithGroupId(string groupId)
        {
            return WithConfiguration(ConfigurationKey.GroupId, groupId);
        }

        public ConsumerConfigurationBuilder WithBootstrapServers(string bootstrapServers)
        {
            return WithConfiguration(ConfigurationKey.BootstrapServers, bootstrapServers);
        }

        public ConsumerConfigurationBuilder WithUnitOfWorkFactory(IHandlerUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            return this;
        }

        internal ConsumerConfigurationBuilder WithConsumerScopeFactory(Func<ILoggerFactory, IConsumerScopeFactory> consumerScopeFactory)
        {
            _consumerScopeFactory = consumerScopeFactory;
            return this;
        }

        public ConsumerConfigurationBuilder ReadFromBeginning(bool readFromBeginning)
        {
            _readFromBeginning = readFromBeginning;
            return this;
        }


        public ConsumerConfigurationBuilder RegisterMessageHandler<TMessage, TMessageHandler>(string topic, string messageType)
            where TMessage : class, new()
            where TMessageHandler : IMessageHandler<TMessage>
        {
            _messageHandlerRegistry.Register<TMessage, TMessageHandler>(topic, messageType);
            return this;
        }

        public ConsumerConfigurationBuilder WithIncomingMessageFactory(IIncomingMessageFactory incomingMessageFactory)
        {
            _incomingMessageFactory = incomingMessageFactory;
            return this;
        }

        internal ConsumerConfiguration Build()
        {
            var configurations = new ConfigurationBuilder()
                .WithConfigurationKeys(DefaultConfigurationKeys)
                .WithRequiredConfigurationKeys(RequiredConfigurationKeys)
                .WithNamingConventions(_namingConventions.ToArray())
                .WithConfigurationSource(_configurationSource)
                .WithConfigurations(_configurations)
                .Build();

            if (_consumerScopeFactory == null)
            {
                _consumerScopeFactory = loggerFactory => new KafkaBasedConsumerScopeFactory(
                    loggerFactory: loggerFactory,
                    configuration: configurations,
                    topics: _messageHandlerRegistry.GetAllSubscribedTopics(),
                    incomingMessageFactory: _incomingMessageFactory,
                    readFromBeginning: _readFromBeginning
                );
            }

            return new ConsumerConfiguration(
                configuration: configurations,
                messageHandlerRegistry: _messageHandlerRegistry,
                unitOfWorkFactory: _unitOfWorkFactory,
                consumerScopeFactory: _consumerScopeFactory,
                resetOffset: _readFromBeginning 
            );
        }
    }
}