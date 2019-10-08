namespace Dafda.Producing
{
    internal class OutgoingMessageBuilder
    {
        private string _topic;
        private string _messageId;
        private string _key;
        private string _type;
        private string _value;

        public OutgoingMessageBuilder()
        {
        }
        
        public OutgoingMessageBuilder WithTopic(string topic)
        {
            _topic = topic;
            return this;
        }

        public OutgoingMessageBuilder WithMessageId(string messageId)
        {
            _messageId = messageId;
            return this;
        }

        public OutgoingMessageBuilder WithKey(string key)
        {
            _key = key;
            return this;
        }

        public OutgoingMessageBuilder WithType(string type)
        {
            _type = type;
            return this;
        }

        public OutgoingMessageBuilder WithValue(string value)
        {
            _value = value;
            return this;
        }

        public OutgoingMessage Build()
        {
            return new OutgoingMessage(_topic, _messageId, _key, _type, _value);
        }

        public static implicit operator OutgoingMessage(OutgoingMessageBuilder builder)
        {
            return builder.Build();
        }
    }
}