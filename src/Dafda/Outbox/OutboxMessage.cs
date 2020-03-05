using System;

namespace Dafda.Outbox
{
    public class OutboxMessage
    {
        public OutboxMessage(Guid messageId, string topic, string key, string payload, DateTime occurredUtc)
        {
            MessageId = messageId;
            Topic = topic;
            Key = key;
            Payload = payload;
            OccurredUtc = occurredUtc;
            ProcessedUtc = null;
        }

        protected OutboxMessage()
        {
        }

        public Guid MessageId { get; private set; }
        public string Topic { get; private set; }
        public string Key { get; private set; }
        public string Payload { get; private set; }
        public DateTime OccurredUtc { get; private set; }
        public DateTime? ProcessedUtc { get; private set; }

        public void MaskAsProcessed()
        {
            ProcessedUtc = DateTime.UtcNow;
        }
    }
}