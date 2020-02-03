using System;
using System.Collections.Generic;
using System.Linq;

namespace Dafda.Consuming
{
    public class MessageHandlerRegistry : IMessageHandlerRegistry
    {
        private readonly List<MessageRegistration> _registrations = new List<MessageRegistration>();

        public MessageRegistration Register<TMessage, THandler>(string topic, string messageType) 
            where THandler : IMessageHandler<TMessage> 
            where TMessage : class, new()
        {
            return Register(
                handlerInstanceType: typeof(THandler),
                messageInstanceType: typeof(TMessage),
                topic: topic,
                messageType: messageType
            );
        }

        public MessageRegistration Register(Type handlerInstanceType, Type messageInstanceType, string topic, string messageType) 
        {
            var registration = new MessageRegistration(
                handlerInstanceType: handlerInstanceType,
                messageInstanceType: messageInstanceType,
                topic: topic,
                messageType: messageType
            );

            _registrations.Add(registration);

            return registration;
        }

        public IEnumerable<string> GetAllSubscribedTopics() => _registrations.Select(x => x.Topic).Distinct();

        public IEnumerable<MessageRegistration> Registrations => _registrations;
        
        public MessageRegistration GetRegistrationFor(string messageType)
        {
            return _registrations.SingleOrDefault(x => x.MessageType == messageType);
        }
    }
}