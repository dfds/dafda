using Dafda.Consuming.Interfaces;
using Dafda.Consuming;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dafda.Configuration
{
    public abstract class ConsumerConfigurationBase
    {
        public ConsumerConfigurationBase(
            IDictionary<string, string> configuration,
            IHandlerUnitOfWorkFactory unitOfWorkFactory,
            IConsumerErrorHandler consumerErrorHandler)
        {
            KafkaConfiguration = configuration;
            UnitOfWorkFactory = unitOfWorkFactory;
            ConsumerErrorHandler = consumerErrorHandler;
        }

        public IDictionary<string, string> KafkaConfiguration { get; }

        public IHandlerUnitOfWorkFactory UnitOfWorkFactory { get; }

        public string GroupId => KafkaConfiguration[ConfigurationKey.GroupId];

        public IConsumerErrorHandler ConsumerErrorHandler { get; }

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
