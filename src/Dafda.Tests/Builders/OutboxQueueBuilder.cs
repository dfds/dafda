using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Outbox;
using Dafda.Producing;
using Dafda.Serializing;

namespace Dafda.Tests.Builders
{
    internal class OutboxQueueBuilder
    {
        private OutgoingMessageRegistry _outgoingMessageRegistry = new OutgoingMessageRegistry();
        private IOutboxEntryRepository _outboxEntryRepository = new DummyOutboxEntryRepository();
        private IPayloadSerializer _payloadSerializer = new DefaultPayloadSerializer();

        public OutboxQueueBuilder With(OutgoingMessageRegistry outgoingMessageRegistry)
        {
            _outgoingMessageRegistry = outgoingMessageRegistry;
            return this;
        }

        public OutboxQueueBuilder With(IOutboxEntryRepository outboxEntryRepository)
        {
            _outboxEntryRepository = outboxEntryRepository;
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
                repository: _outboxEntryRepository,
                outboxNotifier: new NullOutboxNotifier(),
                serializerRegistry: new TopicPayloadSerializerRegistry(() => _payloadSerializer)
            );
        }

        public static implicit operator OutboxQueue(OutboxQueueBuilder builder)
        {
            return builder.Build();
        }

        private class DummyOutboxEntryRepository : IOutboxEntryRepository
        {
            public Task Add(IEnumerable<OutboxEntry> outboxEntries)
            {
                return Task.CompletedTask;
            }
        }

        private class NullOutboxNotifier : IOutboxNotifier
        {
            public Task Notify(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}