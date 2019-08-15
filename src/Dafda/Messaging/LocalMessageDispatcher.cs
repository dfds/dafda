using System.Threading.Tasks;

namespace Dafda.Messaging
{
    public class LocalMessageDispatcher : ILocalMessageDispatcher
    {
        private readonly IMessageHandlerRegistry _handlerRegistry;
        private readonly ITypeResolver _typeResolver;

        public LocalMessageDispatcher(IMessageHandlerRegistry handlerRegistry, ITypeResolver typeResolver)
        {
            _handlerRegistry = handlerRegistry;
            _typeResolver = typeResolver;
        }

        public Task Dispatch(ITransportLevelMessage message)
        {
            var registration = _handlerRegistry.GetRegistrationFor(message.Type);
            if (registration == null)
            {
                throw new MissingMessageHandlerException($"Error! A handler has not been registered for messages of type \"{message.Type}\". Message \"{message.MessageId}\" was not handled.");
            }

            var handler = _typeResolver.Resolve(registration.HandlerInstanceType);
            if (handler == null)
            {
                throw new UnableToResolveMessageHandlerException($"Type resolver {_typeResolver.GetType().FullName} resolved handler type {registration.HandlerInstanceType.FullName} to NULL.");
            }

            var messageInstance = message.ReadDataAs(registration.MessageInstanceType);

            return ExecuteHandler((dynamic)messageInstance, (dynamic)handler);
        }

        private static Task ExecuteHandler<TMessage>(TMessage message, IMessageHandler<TMessage> handler) where TMessage : class, new()
        {
            return handler.Handle(message);
        }
    }
}