using System.Threading.Tasks;
using Dafda.Logging;

namespace Dafda.Producing
{
    public class Bus : IBus
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        private readonly OutgoingMessageFactory _outgoingMessageFactory = new OutgoingMessageFactory();
        private readonly IProducer _producer;

        public Bus(IProducer producer)
        {
            _producer = producer;
        }

        public async Task Publish<TMessage>(TMessage msg) where TMessage : IMessage
        {
            var outgoingMessage = _outgoingMessageFactory.Create(msg);

            await _producer.Produce(outgoingMessage);

            Log.Debug("Message {Name} was published", typeof(TMessage).Name);
        }
    }
}