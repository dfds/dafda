using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Dafda.Configuration;

namespace Dafda.Consuming
{
    public class ConsumerFactory : IConsumerFactory
    {
        public IConsumer CreateConsumer(IConfiguration configuration, IEnumerable<string> topics)
        {
            var consumer = new ConsumerBuilder<string, string>(configuration).Build();
            consumer.Subscribe(topics);

            return new KafkaConsumerWrapper(consumer);
        }
    }
}