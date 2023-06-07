using System;
using System.Collections.Generic;
using Avro.Specific;
using Dafda.Consuming;
using Dafda.Consuming.Avro;
using Dafda.Consuming.Handlers;
using Dafda.Consuming.Interfaces;
using Dafda.Consuming.MessageFilters;

namespace Dafda.Configuration.ConsumerConfigurations
{
    internal abstract class ConsumerConfigurationBase
    {
        public ConsumerConfigurationBase(
            IDictionary<string, string> configuration,
            IHandlerUnitOfWorkFactory unitOfWorkFactory,
            ConsumerErrorHandler consumerErrorHandler)
        {
            KafkaConfiguration = configuration;
            UnitOfWorkFactory = unitOfWorkFactory;

            ConsumerErrorHandler = consumerErrorHandler;
        }

        public IDictionary<string, string> KafkaConfiguration { get; }

        public IHandlerUnitOfWorkFactory UnitOfWorkFactory { get; }

        public string GroupId => KafkaConfiguration[ConfigurationKey.GroupId];

        public ConsumerErrorHandler ConsumerErrorHandler { get; }

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
            ConsumerErrorHandler consumerErrorHandler) : base(configuration, unitOfWorkFactory, consumerErrorHandler)
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

    internal class ConsumerConfiguration<TKey, TValue> : ConsumerConfigurationBase where TValue : ISpecificRecord
    {
        public ConsumerConfiguration(IDictionary<string, string> configuration,
            MessageRegistration<TKey, TValue> messageRegistration,
            IHandlerUnitOfWorkFactory unitOfWorkFactory,
            Func<IServiceProvider, IConsumerScopeFactory<MessageResult<TKey, TValue>>> consumerScopeFactory,
            ConsumerErrorHandler consumerErrorHandler) : base(configuration, unitOfWorkFactory, consumerErrorHandler)
        {
            AvroConsumerScopeFactory = consumerScopeFactory;
            MessageRegistration = messageRegistration;
        }
        public Func<IServiceProvider, IConsumerScopeFactory<MessageResult<TKey, TValue>>> AvroConsumerScopeFactory { get; }
        public MessageRegistration<TKey, TValue> MessageRegistration { get; }
    }
}