using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dafda.Producing;
using Dafda.Serializing;

namespace Dafda.Outbox
{
    public sealed class OutboxQueue
    {
        private readonly IOutboxEntryRepository _repository;
        private readonly IOutboxNotifier _outboxNotifier;
        private readonly TopicPayloadSerializerRegistry _serializerRegistry;
        private readonly PayloadDescriptorFactory _payloadDescriptorFactory;

        internal OutboxQueue(MessageIdGenerator messageIdGenerator, OutgoingMessageRegistry outgoingMessageRegistry, IOutboxEntryRepository repository, IOutboxNotifier outboxNotifier, TopicPayloadSerializerRegistry serializerRegistry)
        {
            _repository = repository;
            _outboxNotifier = outboxNotifier;
            _serializerRegistry = serializerRegistry;
            _payloadDescriptorFactory = new PayloadDescriptorFactory(outgoingMessageRegistry, messageIdGenerator);
        }

        public async Task<IOutboxNotifier> Enqueue(IEnumerable<object> messages)
        {
            var entries = new LinkedList<OutboxEntry>();

            foreach (var message in messages)
            {
                var entry = await CreateOutboxEntry(message);
                entries.AddLast(entry);
            }

            await _repository.Add(entries);

            return _outboxNotifier;
        }

        private async Task<OutboxEntry> CreateOutboxEntry(object message)
        {
            var payloadDescriptor = _payloadDescriptorFactory.Create(message, new Dictionary<string, object>());

            var messageId = Guid.Parse(payloadDescriptor.MessageId);

            var payloadSerializer = _serializerRegistry.Get(payloadDescriptor.TopicName);

            return new OutboxEntry(
                messageId: messageId,
                topic: payloadDescriptor.TopicName,
                key: payloadDescriptor.PartitionKey,
                payload: await payloadSerializer.Serialize(payloadDescriptor),
                occurredUtc: DateTime.UtcNow
            );
        }
    }
}