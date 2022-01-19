using System;
using System.Collections.Generic;
using Dafda.Consuming;
using Dafda.Consuming.MessageFilters;

namespace Dafda.Configuration
{
    internal class ConsumerConfiguration
    {
        public ConsumerConfiguration(
            IDictionary<string, string> configuration,
            MessageHandlerRegistry messageHandlerRegistry,
            IHandlerUnitOfWorkFactory unitOfWorkFactory,
            Func<IServiceProvider, IConsumerScopeFactory> consumerScopeFactory,
            Func<IServiceProvider, IIncomingMessageFactory> incomingMessageFactory,
            MessageFilter messageFilter)
        {
            KafkaConfiguration = configuration;
            MessageHandlerRegistry = messageHandlerRegistry;
            UnitOfWorkFactory = unitOfWorkFactory;
            ConsumerScopeFactory = consumerScopeFactory;
            IncomingMessageFactory = incomingMessageFactory;
            MessageFilter = messageFilter;
        }

        public IDictionary<string, string> KafkaConfiguration { get; }
        public MessageHandlerRegistry MessageHandlerRegistry { get; }
        public IHandlerUnitOfWorkFactory UnitOfWorkFactory { get; }
        public Func<IServiceProvider, IConsumerScopeFactory> ConsumerScopeFactory { get; }
        public Func<IServiceProvider, IIncomingMessageFactory> IncomingMessageFactory { get; }

        public string GroupId => KafkaConfiguration[ConfigurationKey.GroupId];

        public MessageFilter MessageFilter { get; }

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
}