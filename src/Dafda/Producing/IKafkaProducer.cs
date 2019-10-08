using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Dafda.Producing
{
    public interface IKafkaProducer : IDisposable
    {
        Task Produce(string topic, Message<string, string> message);
    }
}