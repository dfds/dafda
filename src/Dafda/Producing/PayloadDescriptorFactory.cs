using System;
using System.Collections.Generic;
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

            return new PayloadDescriptor(
                messageId: _messageIdGenerator.NextMessageId(),
                topicName: registration.Topic,
                partitionKey: registration.KeySelector(message),
                messageType: registration.Type,
                messageData: message,
                messageHeaders: headers.AsEnumerable()
            );
        }


        public PayloadDescriptor Create(object message, Dictionary<string, string> headers)
        {
            return Create(message, new Metadata(headers));
        }
    }
}