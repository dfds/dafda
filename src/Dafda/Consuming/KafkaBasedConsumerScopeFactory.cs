using System.Collections.Generic;
using Confluent.Kafka;

namespace Dafda.Consuming
{
    internal class KafkaBasedConsumerScopeFactory : IConsumerScopeFactory
    {
        private readonly IEnumerable<KeyValuePair<string, string>> _configuration;
        private readonly IEnumerable<string> _topics;

        public KafkaBasedConsumerScopeFactory(IEnumerable<KeyValuePair<string, string>> configuration, IEnumerable<string> topics)
        {
            _configuration = configuration;
            _topics = topics;
        }
        
        public ConsumerScope CreateConsumerScope()
        {
            var consumer = new ConsumerBuilder<string, string>(_configuration).Build();
            consumer.Subscribe(_topics);

            return new KafkaConsumerScope(consumer, new IncomingMessageFactory());
        }
    }
}