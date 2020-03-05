using System.Threading.Tasks;
using Dafda.Outbox;

namespace Dafda.Producing
{
    public sealed class OutboxProducer
    {
        private readonly KafkaProducer _kafkaProducer;

        internal OutboxProducer(KafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
        }

        public async Task Produce(OutboxEntry entry)
        {
            await _kafkaProducer.InternalProduce(entry.Topic, entry.Key, entry.Payload);
        }
    }
}