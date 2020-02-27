using System.Collections.Generic;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Outbox;
using Dafda.Producing;

namespace Dafda.Tests.Builders
{
    internal class OutboxQueueBuilder
    {
        private OutgoingMessageRegistry _outgoingMessageRegistry = new OutgoingMessageRegistry();
        private IOutboxMessageRepository _outboxMessageRepository = new DummyOutboxMessageRepository();
        private IPayloadSerializer _payloadSerializer = new DefaultPayloadSerializer();

        public OutboxQueueBuilder With(OutgoingMessageRegistry outgoingMessageRegistry)
        {
            _outgoingMessageRegistry = outgoingMessageRegistry;
            return this;
        }

        public OutboxQueueBuilder With(IOutboxMessageRepository outboxMessageRepository)
        {
            _outboxMessageRepository = outboxMessageRepository;
            return this;
        }

        public OutboxQueueBuilder With(IPayloadSerializer payloadSerializer)
        {
            _payloadSerializer = payloadSerializer;
            return this;
        }

        public OutboxQueue Build()
        {
            return new OutboxQueue(
                messageIdGenerator: MessageIdGenerator.Default,
                outgoingMessageRegistry: _outgoingMessageRegistry,
                repository: _outboxMessageRepository,
                outboxNotifier: new NullOutboxNotifier(),
                serializerRegistry: new TopicPayloadSerializerRegistry(() => _payloadSerializer)
            );
        }

        public static implicit operator OutboxQueue(OutboxQueueBuilder builder)
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

        private class NullOutboxNotifier : IOutboxNotifier
        {
            public void Notify()
            {
            }
        }
    }
}