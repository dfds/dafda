using System;
using Dafda.Outbox;

namespace Dafda.Tests.Builders
{
    internal class OutboxEntryBuilder
    {
        private string _topic;
        private string _key;
        private string _value;

        public OutboxEntryBuilder WithTopic(string topic)
        {
            _topic = topic;
            return this;
        }

        public OutboxEntryBuilder WithKey(string key)
        {
            _key = key;
            return this;
        }

        public OutboxEntryBuilder WithValue(string value)
        {
            _value = value;
            return this;
        }

        public OutboxEntry Build()
        {
            return new OutboxEntry(Guid.Empty, _topic, _key, _value, DateTime.UtcNow);
        }

        public static implicit operator OutboxEntry(OutboxEntryBuilder builder)
        {
            return builder.Build();
        }
    }
}