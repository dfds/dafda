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
        private readonly IMessageHandlerExecutionStrategy _messageHandlerExecutionStrategy;
        private readonly IDeadLetterQueue _deadLetterQueue;
        private readonly int _maxRetries;
        private readonly Func<Exception, bool> _deadLetterQueueBypass;
        private readonly Func<int, TimeSpan> _retryBackoff;

        public LocalMessageDispatcher(
            MessageHandlerRegistry messageHandlerRegistry,
            IHandlerUnitOfWorkFactory handlerUnitOfWorkFactory,
            IUnconfiguredMessageHandlingStrategy fallbackHandler,
            IMessageHandlerExecutionStrategy messageHandlerExecutionStrategy,
            IDeadLetterQueue deadLetterQueue = null,
            int maxRetries = 0,
            Func<Exception, bool> deadLetterQueueBypass = null,
            Func<int, TimeSpan> retryBackoff = null)
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
            _messageHandlerExecutionStrategy = messageHandlerExecutionStrategy
                ?? throw new ArgumentNullException(nameof(messageHandlerExecutionStrategy));
            _deadLetterQueue = deadLetterQueue;
            _maxRetries = maxRetries;
            _deadLetterQueueBypass = deadLetterQueueBypass;
            _retryBackoff = retryBackoff;
        }

        private MessageRegistration GetMessageRegistrationFor(MessageResult messageResult)
        {
            return _messageHandlerRegistry.GetRegistrationFor(messageResult.Topic, messageResult.Message.Metadata.Type)
            ?? _fallbackHandler.GetFallback(messageResult.Message.Metadata.Type);
        }

        public async Task Dispatch(MessageResult messageResult, CancellationToken cancellationToken)
        {
            // Finding the handler and creating its unit of work are configuration concerns, so they
            // happen outside the try and are never retried or dead-lettered.
            var registration = GetMessageRegistrationFor(messageResult);
            var attempt = 0;

            while (true)
            {
                var unitOfWork = _unitOfWorkFactory.CreateForHandlerType(registration.HandlerInstanceType)
                    ?? throw new UnableToResolveUnitOfWorkForHandlerException($"Error! Unable to create unit of work for handler type \"{registration.HandlerInstanceType.FullName}\".");

                try
                {
                    await Process(messageResult, registration, unitOfWork, cancellationToken);
                    return;
                }
                catch (Exception exception) when (ShouldDeadLetter(exception, cancellationToken))
                {
                    if (attempt < _maxRetries)
                    {
                        attempt++;

                        var delay = _retryBackoff?.Invoke(attempt) ?? TimeSpan.Zero;
                        if (delay > TimeSpan.Zero)
                        {
                            await Task.Delay(delay, cancellationToken);
                        }

                        continue;
                    }

                    await _deadLetterQueue.Send(messageResult, exception, cancellationToken);
                    return;
                }
            }
        }

        private bool ShouldDeadLetter(Exception exception, CancellationToken cancellationToken)
        {
            // InvalidMessageHandlerException is thrown inside Run when the handler cannot be created,
            // so no handler failed and the message is not dead-lettered.
            return _deadLetterQueue != null
                && !cancellationToken.IsCancellationRequested
                && exception is not InvalidMessageHandlerException
                && !(_deadLetterQueueBypass?.Invoke(exception) ?? false);
        }

        private async Task Process(MessageResult messageResult, MessageRegistration registration, IHandlerUnitOfWork unitOfWork, CancellationToken cancellationToken)
        {
            var message = messageResult.Message;
            var messageInstance = message.ReadDataAs(registration.MessageInstanceType);
            var handlerContext = new MessageHandlerContext(message.Metadata);
            var executionContext = new MessageExecutionContext(messageInstance, message.Metadata, registration.MessageInstanceType);
            await unitOfWork.Run(async (handler, cancellationToken) =>
            {
                if (handler == null)
                {
                    throw new InvalidMessageHandlerException($"Error! Message handler of type \"{registration.HandlerInstanceType.FullName}\" not instantiated in unit of work and message instance type of \"{registration.MessageInstanceType}\" for message type \"{registration.MessageType}\" can therefor not be handled.");
                }

                await _messageHandlerExecutionStrategy.Execute(
                    ct => ExecuteHandler((dynamic)messageInstance, (dynamic)handler, handlerContext, ct),
                    executionContext,
                    cancellationToken);
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