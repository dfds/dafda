using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Logging;

namespace Dafda.Producing
{
    public class Producer : IProducer
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        private readonly OutgoingMessageFactory _outgoingMessageFactory;
        private readonly IKafkaProducer _kafkaProducer;

        public Producer(IKafkaProducer kafkaProducer, IProducerConfiguration configuration)
        {
            _kafkaProducer = kafkaProducer;
            _outgoingMessageFactory = new OutgoingMessageFactory(configuration.MessageIdGenerator, configuration.OutgoingMessageRegistry);
        }

        public async Task Produce(object message)
        {
            var outgoingMessage = _outgoingMessageFactory.Create(message);

            await _kafkaProducer.Produce(outgoingMessage);

            Log.Debug("Message for {Type} with id {MessageId} was published", outgoingMessage.Type, outgoingMessage.MessageId);
        }

        public void Dispose()
        {
            _kafkaProducer?.Dispose();
        }
    }
}