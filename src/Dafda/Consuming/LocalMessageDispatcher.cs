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
            _messageHandlerRegistry = messageHandlerRegistry ?? throw new ArgumentNullException(nameof(messageHandlerRegistry));
            _unitOfWorkFactory = handlerUnitOfWorkFactory ?? throw new ArgumentNullException(nameof(handlerUnitOfWorkFactory));
        }

        private MessageRegistration GetMessageRegistrationFor(ITransportLevelMessage message)
        {
            var messageId = message.Metadata.MessageId;
            var messageType = message.Metadata.Type;
            var registration = _messageHandlerRegistry.GetRegistrationFor(messageType);

            if (registration == null)
            {
                throw new MissingMessageHandlerRegistrationException($"Error! A handler has not been registered for messages of type \"{messageType}\". Message with id \"{messageId}\" was not handled.");
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
            var context = new MessageHandlerContext(message.Metadata);

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