using System.Collections.Generic;
using System.Threading.Tasks;
using Dafda.Outbox;
using Dafda.Producing;

namespace Dafda.Tests.Builders
{
    internal class OutboxMessageCollectorBuilder
    {
        private IOutgoingMessageRegistry _outgoingMessageRegistry = new OutgoingMessageRegistry();
        private IOutboxMessageRepository _outboxMessageRepository = new DummyOutboxMessageRepository();

        public OutboxMessageCollectorBuilder With(IOutgoingMessageRegistry outgoingMessageRegistry)
        {
            _outgoingMessageRegistry = outgoingMessageRegistry;
            return this;
        }

        public OutboxMessageCollectorBuilder With(IOutboxMessageRepository outboxMessageRepository)
        {
            _outboxMessageRepository = outboxMessageRepository;
            return this;
        }

        public OutboxMessageCollector Build()
        {
            return new OutboxMessageCollector(MessageIdGenerator.Default, _outgoingMessageRegistry, _outboxMessageRepository);
        }

        public static implicit operator OutboxMessageCollector(OutboxMessageCollectorBuilder builder)
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