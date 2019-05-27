using System;
using System.Threading.Tasks;

namespace Dafda.Messaging
{
    public class LocalMessageDispatcher
    {
        private readonly MessageHandlerRegistry _handlerRegistry;
        private readonly ITypeResolver _typeResolver;

        public LocalMessageDispatcher(MessageHandlerRegistry handlerRegistry, ITypeResolver typeResolver)
        {
            _handlerRegistry = handlerRegistry;
            _typeResolver = typeResolver;
        }

        public Task Dispatch(MessageEmbeddedDocument message)
        {
            var registration = _handlerRegistry.GetRegistrationFor(message.Type);

            if (registration == null)
            {
                throw new Exception($"Error! A handler has not been registered for messages of type \"{message.Type}\". Message \"{message.MessageId}\" was not handled.");
            }

            var messageInstance = message.ReadDataAs(registration.MessageInstanceType);
            var handler = _typeResolver.Resolve(registration.HandlerInstanceType);
            
            return ExecuteHandler((dynamic)messageInstance, (dynamic)handler);
        }

        private static Task ExecuteHandler<TMessage>(TMessage message, IMessageHandler<TMessage> handler) where TMessage : class, new()
        {
            return handler.Handle(message);
        }
    }
}