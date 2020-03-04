using Dafda.Outbox;
using Dafda.Producing;
using Dafda.Serializing;

namespace Dafda.Configuration
{
    internal class OutboxConfiguration
    {
        public OutboxConfiguration(MessageIdGenerator messageIdGenerator, IOutboxNotifier notifier, TopicPayloadSerializerRegistry topicPayloadSerializerRegistry)
        {
            MessageIdGenerator = messageIdGenerator;
            Notifier = notifier;
            TopicPayloadSerializerRegistry = topicPayloadSerializerRegistry;
        }

        public MessageIdGenerator MessageIdGenerator { get; }
        public IOutboxNotifier Notifier { get; }
        public TopicPayloadSerializerRegistry TopicPayloadSerializerRegistry { get; }
    }
}