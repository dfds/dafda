using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Producing;
using Dafda.Serializing;

namespace Dafda.Outbox
{
    public sealed class OutboxQueue
    {
        private readonly IOutboxMessageRepository _repository;
        private readonly IOutboxNotifier _outboxNotifier;
        private readonly TopicPayloadSerializerRegistry _serializerRegistry;
        private readonly PayloadDescriptorFactory _payloadDescriptorFactory;

        internal OutboxQueue(MessageIdGenerator messageIdGenerator, OutgoingMessageRegistry outgoingMessageRegistry, IOutboxMessageRepository repository, IOutboxNotifier outboxNotifier, TopicPayloadSerializerRegistry serializerRegistry)
        {
            _repository = repository;
            _outboxNotifier = outboxNotifier;
            _serializerRegistry = serializerRegistry;
            _payloadDescriptorFactory = new PayloadDescriptorFactory(outgoingMessageRegistry, messageIdGenerator);
        }

        public async Task<IOutboxNotifier> Enqueue(IEnumerable<object> messages)
        {
            var outboxMessages = new LinkedList<OutboxMessage>();

            foreach (var @event in messages)
            {
                var message = await CreateOutboxMessage(@event);
                outboxMessages.AddLast(message);
            }

            await _repository.Add(outboxMessages);

            return _outboxNotifier;
        }

        private async Task<OutboxMessage> CreateOutboxMessage(object message)
        {
            var payloadDescriptor = _payloadDescriptorFactory.Create(message, new Dictionary<string, object>());

            var messageId = Guid.Parse(payloadDescriptor.MessageId);
            var correlationId = Guid.NewGuid().ToString();

            var payloadSerializer = _serializerRegistry.Get(payloadDescriptor.TopicName);

            return new OutboxMessage(
                messageId: messageId,
                correlationId: correlationId,
                topic: payloadDescriptor.TopicName,
                key: payloadDescriptor.PartitionKey,
                type: payloadDescriptor.MessageType,
                format: payloadSerializer.PayloadFormat,
                data: await payloadSerializer.Serialize(payloadDescriptor),
                occurredOnUtc: DateTime.UtcNow
            );
        }
    }
}