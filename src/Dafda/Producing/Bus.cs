using System;
using System.Reflection;
using System.Threading.Tasks;
using Dafda.Logging;

namespace Dafda.Producing
{
    public class Bus : IBus
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        private readonly IProducer _producer;

        public Bus(IProducer producer)
        {
            _producer = producer;
        }

        public async Task Publish<TMessage>(TMessage msg) where TMessage : IMessage
        {
            var (topicName, type) = GetMessageMetaData(msg);

            var message = MessagingHelper.CreateMessageFrom(type, msg);

            await _producer.Produce(new OutgoingMessage(topicName, msg.AggregateId, message));

            Log.Debug("Message {Name} was published", typeof(TMessage).Name);
        }

        private static (string topicName, string type) GetMessageMetaData<TMessage>(TMessage msg) where TMessage : IMessage
        {
            var messageAttribute = msg.GetType()
                .GetTypeInfo()
                .GetCustomAttribute<MessageAttribute>();

            if (messageAttribute == null)
            {
                throw new InvalidOperationException($@"Message ""{typeof(TMessage).Name}"" must have a ""{nameof(MessageAttribute)}"" declared.");
            }

            return (messageAttribute.Topic, messageAttribute.Type);
        }
    }
}