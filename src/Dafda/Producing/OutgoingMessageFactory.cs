namespace Dafda.Producing
{
    public class OutgoingMessageFactory
    {
        private readonly MessageIdGenerator _messageIdGenerator;
        private readonly IOutgoingMessageRegistry _outgoingMessageRegistry;

        public OutgoingMessageFactory(MessageIdGenerator messageIdGenerator, IOutgoingMessageRegistry outgoingMessageRegistry)
        {
            _messageIdGenerator = messageIdGenerator;
            _outgoingMessageRegistry = outgoingMessageRegistry;
        }

        public OutgoingMessage Create(object msg)
        {
            var registration = _outgoingMessageRegistry.GetRegistration(msg);
            var topicName = registration.Topic;
            var type = registration.Type;
            var key = registration.KeySelector(msg);
            var messageId = _messageIdGenerator.NextMessageId();
            var rawMessage = CreateRawMessage(messageId, type, msg);

            return new OutgoingMessage(topicName, messageId, key, type, rawMessage);
        }

        private static string CreateRawMessage(string messageId, string type, object data)
        {
            var message = new
            {
                MessageId = messageId,
                Type = type,
                Data = data
            };

            return MessageEnvelopeSerializer.Create(new MessageEnvelope(messageId, type, data));
        }
    }
}