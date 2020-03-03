using System;
using System.Collections.Generic;
using Dafda.Producing;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
    internal class ProducerConfiguration
    {
        public ProducerConfiguration(IDictionary<string, string> configuration, MessageIdGenerator messageIdGenerator, Func<ILoggerFactory, KafkaProducer> kafkaProducerFactory)
        {
            KafkaConfiguration = configuration;
            MessageIdGenerator = messageIdGenerator;
            KafkaProducerFactory = kafkaProducerFactory;
        }

        public IDictionary<string, string> KafkaConfiguration { get; }
        public MessageIdGenerator MessageIdGenerator { get; }
        public Func<ILoggerFactory, KafkaProducer> KafkaProducerFactory { get; }
    }
}