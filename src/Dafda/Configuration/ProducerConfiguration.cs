using System.Collections.Generic;
using Dafda.Producing;

namespace Dafda.Configuration
{
    public sealed class ProducerConfiguration
    {
        internal ProducerConfiguration(IDictionary<string, string> configuration, MessageIdGenerator messageIdGenerator, IOutgoingMessageRegistry outgoingMessageRegistry, IKafkaProducerFactory kafkaProducerFactory)
        {
            KafkaConfiguration = configuration;
            MessageIdGenerator = messageIdGenerator;
            OutgoingMessageRegistry = outgoingMessageRegistry;
            KafkaProducerFactory = kafkaProducerFactory;
        }

        internal IDictionary<string, string> KafkaConfiguration { get; }
        public MessageIdGenerator MessageIdGenerator { get; }
        public IOutgoingMessageRegistry OutgoingMessageRegistry { get; }
        public IKafkaProducerFactory KafkaProducerFactory { get; }
    }
}