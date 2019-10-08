using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Dafda.Logging;

namespace Dafda.Producing.Kafka
{
    internal class KafkaProducer : IKafkaProducer
    {
        public const string MessageIdHeaderName = "messageId";
        public const string TypeHeaderName = "type";

        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();
        private static readonly Encoding Encoding = Encoding.ASCII;

        private readonly IProducer<string, string> _innerKafkaProducer;

        internal KafkaProducer(IProducer<string, string> innerKafkaProducer)
        {
            _innerKafkaProducer = innerKafkaProducer;
        }

        public async Task Produce(OutgoingMessage outgoingMessage)
        {
            try
            {
                var msg = PrepareOutgoingMessage(outgoingMessage);

                Log.Debug("Producing message {Type} with {Key} on {Topic}", outgoingMessage.Type, outgoingMessage.Key, outgoingMessage.Topic);

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
            var prepareOutgoingMessage = new Message<string, string>
            {
                Key = outgoingMessage.Key,
                Value = outgoingMessage.Value,
                Headers = new Headers
                {
                    {MessageIdHeaderName, Encoding.GetBytes(outgoingMessage.MessageId)},
                    {TypeHeaderName, Encoding.GetBytes(outgoingMessage.Type)}
                }
            };
            return prepareOutgoingMessage;
        }

        public void Dispose()
        {
            _innerKafkaProducer?.Dispose();
        }
    }
}