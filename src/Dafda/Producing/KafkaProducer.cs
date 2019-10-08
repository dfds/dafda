using System.Threading.Tasks;
using Confluent.Kafka;
using Dafda.Logging;

namespace Dafda.Producing
{
    internal class KafkaProducer : IKafkaProducer
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        private readonly IProducer<string, string> _innerKafkaProducer;

        internal KafkaProducer(IProducer<string, string> innerKafkaProducer)
        {
            _innerKafkaProducer = innerKafkaProducer;
        }

        public async Task Produce(string topic, Message<string, string> message)
        {
            try
            {
                await _innerKafkaProducer.ProduceAsync(topic, message);
            }
            catch (ProduceException<string, string> e)
            {
                Log.Error(e, "Error publishing message due to: {ErrorReason} ({ErrorCode})", e.Error.Reason, e.Error.Code);
                throw;
            }
        }

        public void Dispose()
        {
            _innerKafkaProducer?.Dispose();
        }
    }
}