using System;
using System.Collections.Generic;
using Dafda.Consuming;
using Dafda.Consuming.Interfaces;
using Dafda.Consuming.MessageFilters;

namespace Dafda.Configuration
{
    /// <summary>
    /// Base class for consumer configuration
    /// </summary>
    public abstract class ConsumerConfigurationBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="unitOfWorkFactory"></param>
        /// <param name="consumerErrorHandler"></param>
        public ConsumerConfigurationBase(
            IDictionary<string, string> configuration,
            IHandlerUnitOfWorkFactory unitOfWorkFactory,
            IConsumerErrorHandler consumerErrorHandler)
        {
            KafkaConfiguration = configuration;
            UnitOfWorkFactory = unitOfWorkFactory;
            ConsumerErrorHandler = consumerErrorHandler;
        }

        /// <summary>Configs for Kafka</summary>
        public IDictionary<string, string> KafkaConfiguration { get; }

        /// <summary>Factory for unit of work</summary>
        public IHandlerUnitOfWorkFactory UnitOfWorkFactory { get; }

        /// <summary>ConsumerGroupId</summary>
        public string GroupId => KafkaConfiguration[ConfigurationKey.GroupId];

        /// <summary>Error handler</summary>
        public IConsumerErrorHandler ConsumerErrorHandler { get; }

        /// <summary>
        /// Indicates whether commits are made manually or automatically
        /// </summary>
        public bool EnableAutoCommit
        {
            get
            {
                const bool defaultAutoCommitStrategy = true;

                if (!KafkaConfiguration.TryGetValue(ConfigurationKey.EnableAutoCommit, out var value))
                {
                    return defaultAutoCommitStrategy;
                }

                return bool.Parse(value);
            }
        }
    }

    internal class ConsumerConfiguration : ConsumerConfigurationBase
    {
        public ConsumerConfiguration(IDictionary<string, string> configuration,
            MessageHandlerRegistry messageHandlerRegistry,
            IHandlerUnitOfWorkFactory unitOfWorkFactory,
            Func<IServiceProvider, IConsumerScopeFactory<MessageResult>> consumerScopeFactory,
            Func<IServiceProvider, IIncomingMessageFactory> incomingMessageFactory,
            MessageFilter messageFilter,
            IConsumerErrorHandler consumerErrorHandler) : base(configuration, unitOfWorkFactory, consumerErrorHandler)
        {
            MessageHandlerRegistry = messageHandlerRegistry;
            ConsumerScopeFactory = consumerScopeFactory;
            MessageFilter = messageFilter;
            IncomingMessageFactory = incomingMessageFactory;
        }

        public Func<IServiceProvider, IConsumerScopeFactory<MessageResult>> ConsumerScopeFactory { get; }
        public Func<IServiceProvider, IIncomingMessageFactory> IncomingMessageFactory { get; }
        public MessageHandlerRegistry MessageHandlerRegistry { get; }
        public MessageFilter MessageFilter { get; }
    }
}