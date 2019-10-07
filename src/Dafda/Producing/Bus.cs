using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Logging;

namespace Dafda.Producing
{
    public class Bus : IBus
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        private readonly OutgoingMessageFactory _outgoingMessageFactory;
        private readonly IProducer _producer;

        public Bus(IProducer producer, IProducerConfiguration configuration)
        {
            _producer = producer;
            _outgoingMessageFactory = new OutgoingMessageFactory(configuration.MessageIdGenerator, configuration.OutgoingMessageRegistry);
        }

        public async Task Publish(object message)
        {
            var outgoingMessage = _outgoingMessageFactory.Create(message);

            await _producer.Produce(outgoingMessage);

            Log.Debug("Message for {Type} with id {MessageId} was published", outgoingMessage.Type, outgoingMessage.MessageId);
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}