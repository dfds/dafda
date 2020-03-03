using System.Collections.Generic;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Dafda.Consuming
{
    internal class KafkaBasedConsumerScopeFactory : IConsumerScopeFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IEnumerable<KeyValuePair<string, string>> _configuration;
        private readonly IEnumerable<string> _topics;
        private readonly IIncomingMessageFactory _incomingMessageFactory;

        public KafkaBasedConsumerScopeFactory(ILoggerFactory loggerFactory, IEnumerable<KeyValuePair<string, string>> configuration, IEnumerable<string> topics, IIncomingMessageFactory incomingMessageFactory)
        {
            _loggerFactory = loggerFactory;
            _configuration = configuration;
            _topics = topics;
            _incomingMessageFactory = incomingMessageFactory;
        }
        
        public ConsumerScope CreateConsumerScope()
        {
            var consumer = new ConsumerBuilder<string, string>(_configuration).Build();
            consumer.Subscribe(_topics);

            return new KafkaConsumerScope(_loggerFactory, consumer, _incomingMessageFactory);
        }
    }
}