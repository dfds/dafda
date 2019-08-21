using System;
using System.Threading.Tasks;

namespace Dafda.Messaging
{
    public class LocalMessageDispatcher : ILocalMessageDispatcher
    {
        private readonly IMessageHandlerRegistry _handlerRegistry;
        private readonly IHandlerUnitOfWorkFactory _handlerUnitOfWorkFactory;

        public LocalMessageDispatcher(IMessageHandlerRegistry handlerRegistry, IHandlerUnitOfWorkFactory handlerUnitOfWorkFactory)
        {
            if (handlerRegistry == null)
            {
                throw new ArgumentNullException(nameof(handlerRegistry));
            }

            if (handlerUnitOfWorkFactory == null)
            {
                throw new ArgumentNullException(nameof(handlerUnitOfWorkFactory));
            }

            _handlerRegistry = handlerRegistry;
            _handlerUnitOfWorkFactory = handlerUnitOfWorkFactory;
        }

        private MessageRegistration GetMessageRegistrationFor(ITransportLevelMessage message)
        {
            var registration = _handlerRegistry.GetRegistrationFor(message.Type);

            if (registration == null)
            {
                throw new MissingMessageHandlerRegistrationException($"Error! A handler has not been registered for messages of type \"{message.Type}\". Message with id \"{message.MessageId}\" was not handled.");
            }

            return registration;
        }

        public async Task Dispatch(ITransportLevelMessage message)
        {
            var registration = GetMessageRegistrationFor(message);
            var messageInstance = message.ReadDataAs(registration.MessageInstanceType);

            var unitOfWork = _handlerUnitOfWorkFactory.CreateForHandlerType(registration.HandlerInstanceType);

            if (unitOfWork == null)
            {
                throw new UnableToResolveUnitOfWorkForHandlerException($"Error! Unable to create unit of work for handler type \"{registration.HandlerInstanceType.FullName}\".");
            }

            await unitOfWork.Run(handler2 => ExecuteHandler((dynamic)messageInstance, (dynamic)handler2));
        }

        private static Task ExecuteHandler<TMessage>(TMessage message, IMessageHandler<TMessage> handler) where TMessage : class, new()
        {
            return handler.Handle(message);
        }
    }
}