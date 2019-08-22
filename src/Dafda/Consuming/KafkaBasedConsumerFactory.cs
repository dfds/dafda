using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Dafda.Configuration;

namespace Dafda.Consuming
{
    public class KafkaBasedConsumerFactory : IInternalConsumerFactory
    {
        public IInternalConsumer CreateConsumer(IConsumerConfiguration configuration)
        {
            var consumer = new ConsumerBuilder<string, string>(configuration).Build();
            consumer.Subscribe(configuration.SubscribedTopics);

            return new KafkaConsumerWrapper(consumer);
        }
    }
}