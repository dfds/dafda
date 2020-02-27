using System;

namespace Dafda.Outbox
{
    public class OutboxMessage
    {
        public OutboxMessage(Guid messageId, string correlationId, string topic, string key, string type, string format, string data, DateTime occurredOnUtc)
        {
            MessageId = messageId;
            CorrelationId = correlationId;
            Topic = topic;
            Key = key;
            Type = type;
            Format = format;
            Data = data;
            OccurredOnUtc = occurredOnUtc;
            ProcessedUtc = null;
        }

        protected OutboxMessage()
        {
        }

        [Obsolete]
        public string CorrelationId { get; private set; }

        [Obsolete]
        public string Type { get; private set; }

        public Guid MessageId { get; private set; }
        public string Topic { get; private set; }
        public string Key { get; private set; }
        public string Format { get; private set; }
        public string Data { get; private set; }
        public DateTime OccurredOnUtc { get; private set; }
        public DateTime? ProcessedUtc { get; private set; }

        public void MaskAsProcessed()
        {
            ProcessedUtc = DateTime.UtcNow;
        }
    }
}