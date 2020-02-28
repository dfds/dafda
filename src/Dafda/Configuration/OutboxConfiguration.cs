using Dafda.Producing;

namespace Dafda.Configuration
{
    internal class OutboxConfiguration
    {
        public OutboxConfiguration(MessageIdGenerator messageIdGenerator)
        {
            MessageIdGenerator = messageIdGenerator;
        }

        public MessageIdGenerator MessageIdGenerator { get; }
    }
}