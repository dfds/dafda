using System;
using System.Text.Json;

namespace Dafda.Producing
{
    internal class OutgoingMessageFactory
    {
        private readonly IOutgoingMessageRegistry _outgoingMessageRegistry;
        private readonly MessageIdGenerator _messageIdGenerator;

        public OutgoingMessageFactory(IOutgoingMessageRegistry outgoingMessageRegistry, MessageIdGenerator messageIdGenerator)
        {
            _outgoingMessageRegistry = outgoingMessageRegistry;
            _messageIdGenerator = messageIdGenerator;
        }

        public OutgoingMessage Create(object message)
        {
            var registration = _outgoingMessageRegistry.GetRegistration(message);
            if (registration == null)
            {
                throw new InvalidOperationException($"No outgoing message registered for '{message.GetType().Name}'");
            }

            var messageId = _messageIdGenerator.NextMessageId();
            var rawMessage = SerializeRawMessage(messageId, registration.Type, message);

            return new OutgoingMessageBuilder()
                .WithTopic(registration.Topic)
                .WithMessageId(messageId)
                .WithKey(registration.KeySelector(message))
                .WithValue(rawMessage)
                .WithType(registration.Type)
                .Build();
        }

        private static string SerializeRawMessage(string messageId, string type, object data)
        {
            var messageEnvelope = new
            {
                messageId,
                type,
                data
            };

            return JsonSerializer.Serialize(messageEnvelope, new JsonSerializerOptions
            {
                IgnoreNullValues = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                
            });
        }
    }
}