using System.Threading.Tasks;
using Confluent.Kafka;
using Dafda.Producing;

namespace Dafda.Tests.TestDoubles
{
    public class KafkaProducerSpy : IKafkaProducer
    {
        public Task Produce(string topic, Message<string,string> message)
        {
            LastTopic = topic;
            LastMessage = message;
            return Task.CompletedTask;
        }

        public string LastTopic { get; private set; }
        public Message<string,string> LastMessage { get; private set; }

        public void Dispose()
        {
        }
    }
}