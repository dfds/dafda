using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Dafda.Logging;

namespace Dafda.Producing
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
            Log.Debug("Producing message {Type} with {Key} on {Topic}", outgoingMessage.Type, outgoingMessage.Key, outgoingMessage.Topic);

            var message = PrepareOutgoingMessage(outgoingMessage);

            try
            {
                await _innerKafkaProducer.ProduceAsync(outgoingMessage.Topic, message);
            }
            catch (ProduceException<string, string> e)
            {
                Log.Error(e, "Error publishing message due to: {ErrorReason} ({ErrorCode})", e.Error.Reason, e.Error.Code);
                throw;
            }
        }

        public static Message<string, string> PrepareOutgoingMessage(OutgoingMessage outgoingMessage)
        {
            return new KafkaMessageBuilder()
                .WithKey(outgoingMessage.Key)
                .WithValue(outgoingMessage.Value)
                .WithHeader(MessageIdHeaderName, outgoingMessage.MessageId)
                .WithHeader(TypeHeaderName, outgoingMessage.Type)
                .Build();
        }

        public void Dispose()
        {
            _innerKafkaProducer?.Dispose();
        }
    }
}