using System;
using System.Collections.Generic;
using Dafda.Producing;

namespace Dafda.Configuration
{
    internal class ProducerConfiguration
    {
        public ProducerConfiguration(IDictionary<string, string> configuration, MessageIdGenerator messageIdGenerator, Func<KafkaProducer> kafkaProducerFactory)
        {
            KafkaConfiguration = configuration;
            MessageIdGenerator = messageIdGenerator;
            KafkaProducerFactory = kafkaProducerFactory;
        }

        public IDictionary<string, string> KafkaConfiguration { get; }
        public MessageIdGenerator MessageIdGenerator { get; }
        public Func<KafkaProducer> KafkaProducerFactory { get; }
    }
}