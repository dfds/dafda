using System.Collections.Generic;
using System.Linq;

namespace Dafda.DomainEvents
{
    public class DomainEventHandlerRegistry : ITopicProvider
    {
        private readonly List<DomainEventRegistration> _registrations = new List<DomainEventRegistration>();

        public DomainEventRegistration Register<TMessage, THandler>(string topic, string messageType) 
            where THandler : IDomainEventHandler<TMessage> 
            where TMessage : IDomainEvent
        {
            var registration = new DomainEventRegistration
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
        public IEnumerable<DomainEventRegistration> Registrations => _registrations;

        public DomainEventRegistration GetRegistrationFor(string messageType)
        {
            return _registrations.SingleOrDefault(x => x.MessageType == messageType);
        }
    }
}