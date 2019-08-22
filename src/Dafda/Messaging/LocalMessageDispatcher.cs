using System;
using System.Threading.Tasks;

namespace Dafda.Messaging
{
    public class LocalMessageDispatcher : ILocalMessageDispatcher
    {
        private readonly IMessageHandlerRegistry _handlerRegistry;
        private readonly IHandlerUnitOfWorkFactory _unitOfWorkFactory;

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
            _unitOfWorkFactory = handlerUnitOfWorkFactory;
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

            var unitOfWork = _unitOfWorkFactory.CreateForHandlerType(registration.HandlerInstanceType);
            if (unitOfWork == null)
            {
                throw new UnableToResolveUnitOfWorkForHandlerException($"Error! Unable to create unit of work for handler type \"{registration.HandlerInstanceType.FullName}\".");
            }

            var messageInstance = message.ReadDataAs(registration.MessageInstanceType);
            await unitOfWork.Run(async handler => await ExecuteHandler((dynamic)messageInstance, (dynamic)handler));
        }

        private static Task ExecuteHandler<TMessage>(TMessage message, IMessageHandler<TMessage> handler) where TMessage : class, new()
        {
            return handler.Handle(message);
        }
    }
}