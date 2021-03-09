using System;
using System.Collections.Generic;
using System.Linq;
using Dafda.Consuming;
using Dafda.Serializing;

namespace Dafda.Producing
{
    internal class PayloadDescriptorFactory
    {
        private readonly OutgoingMessageRegistry _outgoingMessageRegistry;
        private readonly MessageIdGenerator _messageIdGenerator;

        public PayloadDescriptorFactory(OutgoingMessageRegistry outgoingMessageRegistry, MessageIdGenerator messageIdGenerator)
        {
            _outgoingMessageRegistry = outgoingMessageRegistry;
            _messageIdGenerator = messageIdGenerator;
        }

        public PayloadDescriptor Create(object message, Metadata headers)
        {
            var registration = _outgoingMessageRegistry.GetRegistration(message);
            if (registration == null)
            {
                throw new InvalidOperationException($"No outgoing message registered for '{message.GetType().Name}'");
            }

            var messageId = string.IsNullOrEmpty(headers.MessageId) ? _messageIdGenerator.NextMessageId() : headers.MessageId;
            var metadata = new Metadata( headers.AsEnumerable().ToDictionary( k => k.Key, v => v.Value ) )
            {
                CausationId = string.IsNullOrEmpty(headers.CausationId) ? messageId : headers.CausationId,
                CorrelationId = string.IsNullOrEmpty(headers.CorrelationId) ? messageId : headers.CorrelationId,
            };

            return new PayloadDescriptor(
                messageId: messageId,
                topicName: registration.Topic,
                partitionKey: registration.KeySelector(message),
                messageType: registration.Type,
                messageData: message,
                messageHeaders: metadata.AsEnumerable()
            );
        }


        public PayloadDescriptor Create(object message, Dictionary<string, string> headers)
        {
            return Create(message, new Metadata(headers));
        }
    }
}