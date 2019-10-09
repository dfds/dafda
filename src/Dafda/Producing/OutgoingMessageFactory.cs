using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

            return JsonConvert.SerializeObject(messageEnvelope, new JsonSerializerSettings
            {
                Culture = CultureInfo.InvariantCulture,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore
            });
        }
    }
}