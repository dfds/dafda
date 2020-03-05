using System;
using Dafda.Outbox;

namespace Dafda.Tests.Builders
{
    internal class OutboxMessageBuilder
    {
        private string _topic;
        private string _key;
        private string _value;

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

        public OutboxMessageBuilder WithValue(string value)
        {
            _value = value;
            return this;
        }

        public OutboxMessage Build()
        {
            return new OutboxMessage(Guid.Empty, _topic, _key, _value, DateTime.UtcNow);
        }

        public static implicit operator OutboxMessage(OutboxMessageBuilder builder)
        {
            return builder.Build();
        }
    }
}