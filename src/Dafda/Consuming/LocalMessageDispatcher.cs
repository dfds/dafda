using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Consuming
{
    internal class LocalMessageDispatcher
    {
        private readonly MessageHandlerRegistry _messageHandlerRegistry;
        private readonly IHandlerUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IUnconfiguredMessageHandlingStrategy _fallbackHandler;
        private readonly IConsumerExecutionStrategy _consumerExecutionStrategy;

        public LocalMessageDispatcher(
            MessageHandlerRegistry messageHandlerRegistry,
            IHandlerUnitOfWorkFactory handlerUnitOfWorkFactory,
            IUnconfiguredMessageHandlingStrategy fallbackHandler,
            IConsumerExecutionStrategy consumerExecutionStrategy)
        {
            _messageHandlerRegistry =
                messageHandlerRegistry
                ?? throw new ArgumentNullException(nameof(messageHandlerRegistry));
            _unitOfWorkFactory =
                handlerUnitOfWorkFactory
                ?? throw new ArgumentNullException(nameof(handlerUnitOfWorkFactory));
            _fallbackHandler =
                fallbackHandler
                ?? throw new ArgumentNullException(nameof(fallbackHandler));
            _consumerExecutionStrategy = consumerExecutionStrategy
                ?? throw new ArgumentNullException(nameof(consumerExecutionStrategy));
        }

        private MessageRegistration GetMessageRegistrationFor(MessageResult messageResult)
        {
            return _messageHandlerRegistry.GetRegistrationFor(messageResult.Topic, messageResult.Message.Metadata.Type)
            ?? _fallbackHandler.GetFallback(messageResult.Message.Metadata.Type);
        }

        public async Task Dispatch(MessageResult messageResult, CancellationToken cancellationToken)
        {
            var registration = GetMessageRegistrationFor(messageResult);

            var unitOfWork = _unitOfWorkFactory.CreateForHandlerType(registration.HandlerInstanceType);
            if (unitOfWork == null)
            {
                throw new UnableToResolveUnitOfWorkForHandlerException($"Error! Unable to create unit of work for handler type \"{registration.HandlerInstanceType.FullName}\".");
            }

            var message = messageResult.Message;
            var messageInstance = message.ReadDataAs(registration.MessageInstanceType);
            var context = new MessageHandlerContext(message.Metadata);
            await unitOfWork.Run(async (handler ,cancellationToken) =>
            {
                if (handler == null)
                {
                    throw new InvalidMessageHandlerException($"Error! Message handler of type \"{registration.HandlerInstanceType.FullName}\" not instantiated in unit of work and message instance type of \"{registration.MessageInstanceType}\" for message type \"{registration.MessageType}\" can therefor not be handled.");
                }

                // TODO -- verify that the handler is in fact an implementation of IMessageHandler<registration.MessageInstanceType> to provider sane error messages.

                await _consumerExecutionStrategy.Execute(() => ExecuteHandler((dynamic)messageInstance, (dynamic)handler, context, cancellationToken));
            }, cancellationToken);
        }

        private static Task ExecuteHandler<TMessage>(
            TMessage message,
            IMessageHandler<TMessage> handler,
            MessageHandlerContext context,
            CancellationToken cancellationToken)
        {
            return handler.Handle(message, context, cancellationToken);
        }
    }
}