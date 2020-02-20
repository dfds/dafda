using System.Collections.Generic;
using System.Threading.Tasks;
using Dafda.Outbox;
using Dafda.Producing;

namespace Dafda.Tests.Builders
{
    internal class OutboxQueueBuilder
    {
        private IOutgoingMessageRegistry _outgoingMessageRegistry = new OutgoingMessageRegistry();
        private IOutboxMessageRepository _outboxMessageRepository = new DummyOutboxMessageRepository();

        public OutboxQueueBuilder With(IOutgoingMessageRegistry outgoingMessageRegistry)
        {
            _outgoingMessageRegistry = outgoingMessageRegistry;
            return this;
        }

        public OutboxQueueBuilder With(IOutboxMessageRepository outboxMessageRepository)
        {
            _outboxMessageRepository = outboxMessageRepository;
            return this;
        }

        public Dafda.Outbox.OutboxQueue Build()
        {
            return new Dafda.Outbox.OutboxQueue(MessageIdGenerator.Default, _outgoingMessageRegistry, _outboxMessageRepository);
        }

        public static implicit operator Dafda.Outbox.OutboxQueue(OutboxQueueBuilder builder)
        {
            return builder.Build();
        }

        private class DummyOutboxMessageRepository : IOutboxMessageRepository
        {
            public Task Add(IEnumerable<OutboxMessage> outboxMessages)
            {
                return Task.CompletedTask;
            }
        }
    }
}