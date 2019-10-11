using System;
using Dafda.Outbox;

namespace Dafda.Tests.Builders
{
    internal class OutboxMessageBuilder
    {
        private Guid _messageId;
        private string _topic;
        private string _key;
        private string _type;
        private string _value;
        private DateTime _occurredOnUtc;

        public OutboxMessageBuilder WithMessageId(Guid messageId)
        {
            _messageId = messageId;
            return this;
        }

        public OutboxMessageBuilder WithTopic(string topic)
        {
            _topic = topic;
            return this;
        }

        public OutboxMessageBuilder WithKey(string key)
        {
            _key = key;
            return this;
        }

        public OutboxMessageBuilder WithType(string type)
        {
            _type = type;
            return this;
        }

        public OutboxMessageBuilder WithValue(string value)
        {
            _value = value;
            return this;
        }

        public OutboxMessageBuilder OccurredOnUtc(DateTime occurredOnUtc)
        {
            _occurredOnUtc = occurredOnUtc;
            return this;
        }

        public OutboxMessage Build()
        {
            return new OutboxMessage(_messageId, null, _topic, _key, _type, null, _value, _occurredOnUtc);
        }

        public static implicit operator OutboxMessage(OutboxMessageBuilder builder)
        {
            return builder.Build();
        }
    }
}