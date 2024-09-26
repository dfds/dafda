using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Diagnostics;
using Dafda.Producing;
using Dafda.Serializing;

namespace Dafda.Outbox
{
    /// <summary></summary>
    public sealed class OutboxQueue
    {
        private readonly IOutboxEntryRepository _repository;
        private readonly IOutboxNotifier _outboxNotifier;
        private readonly TopicPayloadSerializerRegistry _serializerRegistry;
        private readonly PayloadDescriptorFactory _payloadDescriptorFactory;

        internal OutboxQueue(MessageIdGenerator messageIdGenerator, OutgoingMessageRegistry outgoingMessageRegistry, IOutboxEntryRepository repository, IOutboxNotifier outboxNotifier,
            TopicPayloadSerializerRegistry serializerRegistry)
        {
            _repository = repository;
            _outboxNotifier = outboxNotifier;
            _serializerRegistry = serializerRegistry;
            _payloadDescriptorFactory = new PayloadDescriptorFactory(outgoingMessageRegistry, messageIdGenerator);
        }

        /// <summary>
        /// Send domain events to be processed by the Dafda outbox feature
        /// </summary>
        /// <param name="messages">The list of messages to add to the outbox</param>
        /// <returns>
        /// A <see cref="IOutboxNotifier"/> which can be used to signal the outbox processing mechanism,
        /// whether local or remote. The <see cref="IOutboxNotifier.Notify"/> can be used to signal the
        /// processor when new events are available.
        /// </returns>
        /// <remarks>
        /// Calling <see cref="IOutboxNotifier.Notify"/> can happen as part of a transaction, e.g. when
        /// using Postgres' <c>LISTEN/NOTIFY</c>, or after the transactions has been committed, when using
        /// the built-in <see cref="IOutboxNotifier"/>.
        /// </remarks>
        public async Task<IOutboxNotifier> Enqueue(IEnumerable<object> messages)
        {
            var metadata = new Metadata();

            return await Enqueue(messages, metadata);
        }

        /// <summary>
        /// Send domain events to be processed by the Dafda outbox feature
        /// </summary>
        /// <param name="messages">The list of messages to add to the outbox</param>
        /// <param name="headers">The message headers</param>
        /// <returns>
        /// A <see cref="IOutboxNotifier"/> which can be used to signal the outbox processing mechanism,
        /// whether local or remote. The <see cref="IOutboxNotifier.Notify"/> can be used to signal the
        /// processor when new events are available.
        /// </returns>
        /// <remarks>
        /// Calling <see cref="IOutboxNotifier.Notify"/> can happen as part of a transaction, e.g. when
        /// using Postgres' <c>LISTEN/NOTIFY</c>, or after the transactions has been committed, when using
        /// the built-in <see cref="IOutboxNotifier"/>.
        /// </remarks>
        public async Task<IOutboxNotifier> Enqueue(IEnumerable<object> messages, Metadata headers)
        {
            Activity activity = null;
            if (messages.Any())
            {
                activity = DafdaActivitySource.StartOutboxEnqueueingActivity(headers);
            }

            activity?.Start();


            var entries = new LinkedList<OutboxEntry>();

            foreach (var message in messages)
            {
                var entry = await CreateOutboxEntry(message, headers);
                entries.AddLast(entry);
            }

            await _repository.Add(entries);
            activity?.Dispose();
            return _outboxNotifier;
        }

        private async Task<OutboxEntry> CreateOutboxEntry(object message, Metadata metadata)
        {
            var payloadDescriptor = _payloadDescriptorFactory.Create(message, metadata);

            using var activity = DafdaActivitySource.StartOutboxEntryCreationActivity(payloadDescriptor, metadata);

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