using System;
using System.Collections.Generic;
using Dafda.Consuming;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
    internal class ConsumerConfiguration
    {
        public ConsumerConfiguration(IDictionary<string, string> configuration, MessageHandlerRegistry messageHandlerRegistry, 
            IHandlerUnitOfWorkFactory unitOfWorkFactory, Func<ILoggerFactory, IConsumerScopeFactory> consumerScopeFactory)
        {
            KafkaConfiguration = configuration;
            MessageHandlerRegistry = messageHandlerRegistry;
            UnitOfWorkFactory = unitOfWorkFactory;
            ConsumerScopeFactory = consumerScopeFactory;
        }

        public IDictionary<string, string> KafkaConfiguration { get; }
        public MessageHandlerRegistry MessageHandlerRegistry { get; }
        public IHandlerUnitOfWorkFactory UnitOfWorkFactory { get; }
        public Func<ILoggerFactory, IConsumerScopeFactory> ConsumerScopeFactory { get; }

        public string GroupId => KafkaConfiguration[ConfigurationKey.GroupId];

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