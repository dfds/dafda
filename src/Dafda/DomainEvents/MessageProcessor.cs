using System;
using System.Threading.Tasks;

namespace Dafda.DomainEvents
{
    public class MessageProcessor
    {
        private readonly DomainEventHandlerRegistry _handlerRegistry;
        private readonly ITypeResolver _typeResolver;

        public MessageProcessor(DomainEventHandlerRegistry handlerRegistry, ITypeResolver typeResolver)
        {
            _handlerRegistry = handlerRegistry;
            _typeResolver = typeResolver;
        }

        public Task Process(MessageEmbeddedDocument message)
        {
            // Log.Debug($"Processing message \"{message.MessageId}\" with correlation id \"{message.CorrelationId}\"");
            var registration = _handlerRegistry.GetRegistrationFor(message.Type);

            if (registration == null)
            {
                throw new Exception($"Error! A handler has not been registered for messages of type \"{message.Type}\". message \"{message.MessageId}\" was not handled.");
            }

            var domainEvent = message.ReadDataAs(registration.MessageInstanceType);
            var handler = _typeResolver.Resolve(registration.HandlerInstanceType);
            
            return ExecuteHandler((dynamic)domainEvent, (dynamic)handler);
        }

        private static Task ExecuteHandler<TMessage>(TMessage message, IDomainEventHandler<TMessage> handler) where TMessage : IDomainEvent
        {
            return handler.Handle(message);
        }
    }
}