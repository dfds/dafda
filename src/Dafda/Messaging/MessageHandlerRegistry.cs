using System.Collections.Generic;
using System.Linq;

namespace Dafda.Messaging
{
    public class MessageHandlerRegistry : ITopicProvider
    {
        private readonly List<MessageRegistration> _registrations = new List<MessageRegistration>();

        public MessageRegistration Register<TMessage, THandler>(string topic, string messageType) 
            where THandler : IMessageHandler<TMessage> 
            where TMessage : class, new()
        {
            var registration = new MessageRegistration
            {
                HandlerInstanceType = typeof(THandler),
                MessageInstanceType = typeof(TMessage),
                Topic = topic,
                MessageType = messageType
            };

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