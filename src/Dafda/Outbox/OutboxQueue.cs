using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dafda.Producing;

namespace Dafda.Outbox
{
    public class OutboxQueue
    {
        private readonly IOutboxMessageRepository _repository;
        private readonly OutgoingMessageFactory _outgoingMessageFactory;

        public OutboxQueue(MessageIdGenerator messageIdGenerator, IOutgoingMessageRegistry outgoingMessageRegistry, IOutboxMessageRepository repository)
        {
            _outgoingMessageFactory = new OutgoingMessageFactory(outgoingMessageRegistry, messageIdGenerator);
            _repository = repository;
        }

        public async Task Enqueue(IEnumerable<object> events)
        {
            var outboxMessages = events
                .Select(CreateOutboxMessage)
                .ToArray();

            await _repository.Add(outboxMessages);
        }

        private OutboxMessage CreateOutboxMessage(object @event)
        {
            var outgoingMessage = _outgoingMessageFactory.Create(@event);

            var messageId = outgoingMessage.MessageId;
            var correlationId = Guid.NewGuid().ToString();
            var topic = outgoingMessage.Topic;
            var key = outgoingMessage.Key;
            var type = outgoingMessage.Type;
            var format = "application/json";
            var data = outgoingMessage.Value;

            return new OutboxMessage(Guid.Parse(messageId), correlationId, topic, key, type, format, data, DateTime.UtcNow);
        }
    }
}