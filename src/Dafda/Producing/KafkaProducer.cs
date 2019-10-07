using System.Threading.Tasks;
using Confluent.Kafka;
using Dafda.Logging;

namespace Dafda.Producing
{
    internal class KafkaProducer : IProducer
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        private readonly IProducer<string, string> _innerKafkaProducer;

        internal KafkaProducer(IProducer<string, string> innerKafkaProducer)
        {
            _innerKafkaProducer = innerKafkaProducer;
        }

        public async Task Produce(OutgoingMessage outgoingMessage)
        {
            try
            {
                Log.Debug("Producing message {Type} with {Key} on {Topic}", outgoingMessage.Type, outgoingMessage.Key, outgoingMessage.Topic);

                var message = MessageFactory.Create(outgoingMessage);

                await _innerKafkaProducer.ProduceAsync(outgoingMessage.Topic, message);
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