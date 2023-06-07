using System;
using System.Collections.Generic;
using System.Linq;
using Avro.Specific;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Dafda.Consuming.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dafda.Consuming.Factories
{
    internal class KafkaBasedConsumerScopeFactory : IConsumerScopeFactory<MessageResult>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IEnumerable<KeyValuePair<string, string>> _configuration;
        private readonly IEnumerable<string> _topics;
        private readonly IIncomingMessageFactory _incomingMessageFactory;
        private readonly bool _readFromBeginning;

        public KafkaBasedConsumerScopeFactory(ILoggerFactory loggerFactory, IEnumerable<KeyValuePair<string, string>> configuration, IEnumerable<string> topics, IIncomingMessageFactory incomingMessageFactory, bool readFromBeginning)
        {
            _loggerFactory = loggerFactory;
            _configuration = configuration;
            _topics = topics;
            _incomingMessageFactory = incomingMessageFactory;
            _readFromBeginning = readFromBeginning;
        }

        public IConsumerScope<MessageResult> CreateConsumerScope()
        {
            var consumerBuilder = new ConsumerBuilder<string, string>(_configuration);
            if (_readFromBeginning)
            {
                consumerBuilder.SetPartitionsAssignedHandler((cons, topicPartitions) => { return topicPartitions.Select(tp => new TopicPartitionOffset(tp, Offset.Beginning)); });
            }

            var consumer = consumerBuilder.Build();
            consumer.Subscribe(_topics);

            return new KafkaConsumerScope(_loggerFactory, consumer, _incomingMessageFactory);
        }
    }
}