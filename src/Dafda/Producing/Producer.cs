using System.Threading.Tasks;
using Confluent.Kafka;
using Dafda.Configuration;
using Dafda.Logging;

namespace Dafda.Producing
{
    public class Producer : IProducer
    {
        public const string MessageIdHeaderName = "messageId";
        public const string TypeHeaderName = "type";

        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        private readonly OutgoingMessageFactory _outgoingMessageFactory;
        private readonly IKafkaProducer _kafkaProducer;

        public Producer(IKafkaProducer kafkaProducer, OutgoingMessageFactory outgoingMessageFactory)
        {
            _kafkaProducer = kafkaProducer;
            _outgoingMessageFactory = outgoingMessageFactory;
        }

        public async Task Produce(object message)
        {
            var outgoingMessage = _outgoingMessageFactory.Create(message);
            var msg = PrepareOutgoingMessage(outgoingMessage);

            Log.Debug("Producing message {Type} with {Key} on {Topic}", outgoingMessage.Type, outgoingMessage.Key, outgoingMessage.Topic);

            await _kafkaProducer.Produce(outgoingMessage.Topic, msg);

            Log.Debug("Message for {Type} with id {MessageId} was published", outgoingMessage.Type, outgoingMessage.MessageId);
        }

        public void Dispose()
        {
            _kafkaProducer?.Dispose();
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
    }
}