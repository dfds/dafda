using System;
using System.Threading.Tasks;

namespace Dafda.Consuming
{
    internal class LocalMessageDispatcher
    {
        private readonly MessageHandlerRegistry _messageHandlerRegistry;
        private readonly IHandlerUnitOfWorkFactory _unitOfWorkFactory;

        public LocalMessageDispatcher(MessageHandlerRegistry messageHandlerRegistry, IHandlerUnitOfWorkFactory handlerUnitOfWorkFactory)
        {
            if (messageHandlerRegistry == null)
            {
                throw new ArgumentNullException(nameof(messageHandlerRegistry));
            }

            if (handlerUnitOfWorkFactory == null)
            {
                throw new ArgumentNullException(nameof(handlerUnitOfWorkFactory));
            }

            _messageHandlerRegistry = messageHandlerRegistry;
            _unitOfWorkFactory = handlerUnitOfWorkFactory;
        }

        private MessageRegistration GetMessageRegistrationFor(ITransportLevelMessage message)
        {
            var registration = _messageHandlerRegistry.GetRegistrationFor(message.Type);

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
            
            var context = new MessageHandlerContext(message.MessageId, message.Type);
            
            await unitOfWork.Run(async handler =>
            {
                if (handler == null)
                {
                    throw new InvalidMessageHandlerException($"Error! Message handler of type \"{registration.HandlerInstanceType.FullName}\" not instantiated in unit of work and message instance type of \"{registration.MessageInstanceType}\" for message type \"{registration.MessageType}\" can therefor not be handled.");
                }

                await ExecuteHandler((dynamic) messageInstance, (dynamic) handler, context);
            });
        }

        private static Task ExecuteHandler<TMessage>(TMessage message, IMessageHandler<TMessage> handler, MessageHandlerContext context) where TMessage : class, new()
        {
            return handler.Handle(message, context);
        }
    }
}