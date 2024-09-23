using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Dafda.Diagnostics;
using Dafda.Outbox;

namespace Dafda.Producing
{
    /// <summary>
    /// A specific producer for the Dafda outbox
    /// </summary>
    internal sealed class OutboxProducer
    {
        private readonly KafkaProducer _kafkaProducer;

        internal OutboxProducer(KafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
        }

        /// <summary>
        /// Produce the <see cref="OutboxEntry"/> on Kafka using the data contained
        /// </summary>
        /// <param name="entry">The outbox message</param>
        public async Task Produce(OutboxEntry entry)
        {
            using var activity = DafdaActivitySource.StartPublishingActivity(entry, _kafkaProducer.ClientId);

            await _kafkaProducer.InternalProduce(entry.Topic, entry.Key, entry.Payload);
        }
    }
}