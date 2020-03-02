using Dafda.Outbox;
using Dafda.Producing;

namespace Dafda.Configuration
{
    internal class OutboxConfiguration
    {
        public OutboxConfiguration(MessageIdGenerator messageIdGenerator, IOutboxNotifier notifier)
        {
            MessageIdGenerator = messageIdGenerator;
            Notifier = notifier;
        }

        public MessageIdGenerator MessageIdGenerator { get; }
        public IOutboxNotifier Notifier { get; }
    }
}