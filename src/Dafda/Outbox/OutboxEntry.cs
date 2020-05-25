using System;

namespace Dafda.Outbox
{
    /// <summary>
    /// The <see cref="OutboxEntry"/> represents the serialized version of the outbox message, including 
    /// unique <see cref="MessageId"/>, destination <see cref="Topic"/>, partition <see cref="Key"/>,
    /// and serialized <see cref="Payload"/>.
    /// </summary>
    public class OutboxEntry
    {
        /// <summary>
        /// Initialize an instance of the Outbox entry.
        /// </summary>
        /// <param name="messageId">The unique message id</param>
        /// <param name="topic">The topic name</param>
        /// <param name="key">The partition key</param>
        /// <param name="payload">The serialized payload</param>
        /// <param name="occurredUtc">UTC datetime of when the message was generated</param>
        public OutboxEntry(Guid messageId, string topic, string key, string payload, DateTime occurredUtc)
        {
            MessageId = messageId;
            Topic = topic;
            Key = key;
            Payload = payload;
            OccurredUtc = occurredUtc;
            ProcessedUtc = null;
        }

        /// <summary>
        /// Initialized an instance of the <see cref="OutboxEntry"/>. Mostly left here
        /// to suit EF Core and other ORMs. 
        /// </summary>
        protected OutboxEntry()
        {
        }

        /// <summary>
        /// The unique message id
        /// </summary>
        public Guid MessageId { get; private set; }

        /// <summary>
        /// The topic name
        /// </summary>
        public string Topic { get; private set; }

        /// <summary>
        /// The partition key
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// The serialized payload
        /// </summary>
        public string Payload { get; private set; }

        /// <summary>
        /// UTC datetime of when the message was generated
        /// </summary>
        public DateTime OccurredUtc { get; private set; }

        /// <summary>
        /// UTC datetime of when the message was processed
        /// </summary>
        public DateTime? ProcessedUtc { get; private set; }

        /// <summary>
        /// Mark the <see cref="OutboxEntry"/> as processed
        /// </summary>
        public void MaskAsProcessed()
        {
            ProcessedUtc = DateTime.UtcNow;
        }
    }
}