using System.Threading.Tasks;
using Confluent.Kafka;
using Dafda.Logging;

namespace Dafda.Producing.Kafka
{
    internal class KafkaProducer : IKafkaProducer
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

                var msg = PrepareOutgoingMessage(outgoingMessage);
                await _innerKafkaProducer.ProduceAsync(outgoingMessage.Topic, msg);

                Log.Debug("Message for {Type} with id {MessageId} was published", outgoingMessage.Type, outgoingMessage.MessageId);
            }
            catch (ProduceException<string, string> e)
            {
                Log.Error(e, "Error publishing message due to: {ErrorReason} ({ErrorCode})", e.Error.Reason, e.Error.Code);
                throw;
            }
        }

        public static Message<string, string> PrepareOutgoingMessage(OutgoingMessage outgoingMessage)
        {
            return new Message<string, string>
            {
                Key = outgoingMessage.Key,
                Value = outgoingMessage.Value,
            };
        }

        public void Dispose()
        {
            _innerKafkaProducer?.Dispose();
        }
    }
}