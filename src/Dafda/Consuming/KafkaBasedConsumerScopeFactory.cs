using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Dafda.Configuration;
using Microsoft.Extensions.Logging;

namespace Dafda.Consuming
{
    internal class KafkaBasedConsumerScopeFactory : IConsumerScopeFactory
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
        
        public ConsumerScope CreateConsumerScope()
        {
            var consumerBuilder = new ConsumerBuilder<string, string>(_configuration);
            if (_readFromBeginning)
            {
                consumerBuilder.SetPartitionsAssignedHandler((cons, topicPartitions) => { return topicPartitions.Select(tp => new TopicPartitionOffset(tp, Offset.Beginning)); });
            }

            var consumer = consumerBuilder.Build();
            consumer.Subscribe(_topics);

            // at this stage, groupId must be present in the configuration
            var groupId = _configuration.First(x => x.Key == ConfigurationKey.GroupId).Value;
            return new KafkaConsumerScope(_loggerFactory, consumer, _incomingMessageFactory, groupId);
        }
    }
}