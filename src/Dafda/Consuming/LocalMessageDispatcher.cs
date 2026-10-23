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
        private readonly DeadLetterPolicy _deadLetterPolicy;

        public LocalMessageDispatcher(
            MessageHandlerRegistry messageHandlerRegistry,
            IHandlerUnitOfWorkFactory handlerUnitOfWorkFactory,
            IUnconfiguredMessageHandlingStrategy fallbackHandler,
            IMessageHandlerExecutionStrategy messageHandlerExecutionStrategy,
            DeadLetterPolicy deadLetterPolicy = null)
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
            _deadLetterPolicy = deadLetterPolicy;
        }

        private MessageRegistration GetMessageRegistrationFor(MessageResult messageResult)
        {
            return _messageHandlerRegistry.GetRegistrationFor(messageResult.Topic, messageResult.Message.Metadata.Type)
            ?? _fallbackHandler.GetFallback(messageResult.Message.Metadata.Type);
        }

        public async Task Dispatch(MessageResult messageResult, CancellationToken cancellationToken)
        {
            var registration = GetMessageRegistrationFor(messageResult);
            var messageInstance = messageResult.Message.ReadDataAs(registration.MessageInstanceType);
            var attempt = 0;

            while (true)
            {
                var unitOfWork = CreateUnitOfWork(registration);
                var failure = await RunHandler(unitOfWork, registration, messageResult.Message, messageInstance, cancellationToken);

                if (failure == null)
                {
                    return;
                }

                if (attempt >= _deadLetterPolicy.MaxRetries)
                {
                    await _deadLetterPolicy.Queue.Send(messageResult, failure, cancellationToken);
                    return;
                }

                attempt++;
                await WaitBeforeRetry(attempt, cancellationToken);
            }
        }

        private IHandlerUnitOfWork CreateUnitOfWork(MessageRegistration registration)
        {
            return _unitOfWorkFactory.CreateForHandlerType(registration.HandlerInstanceType)
                ?? throw new UnableToResolveUnitOfWorkForHandlerException($"Error! Unable to create unit of work for handler type \"{registration.HandlerInstanceType.FullName}\".");
        }

        // Returns the handler's exception when it should be retried or dead-lettered, otherwise null.
        // The unit of work creates the handler before the try, so creation failures are thrown as they are.
        private async Task<Exception> RunHandler(IHandlerUnitOfWork unitOfWork, MessageRegistration registration, TransportLevelMessage message, object messageInstance, CancellationToken cancellationToken)
        {
            var handlerContext = new MessageHandlerContext(message.Metadata);
            var executionContext = new MessageExecutionContext(messageInstance, message.Metadata, registration.MessageInstanceType);
            Exception failure = null;

            await unitOfWork.Run(async (handler, cancellationToken) =>
            {
                if (handler == null)
                {
                    throw new InvalidMessageHandlerException($"Error! Message handler of type \"{registration.HandlerInstanceType.FullName}\" not instantiated in unit of work and message instance type of \"{registration.MessageInstanceType}\" for message type \"{registration.MessageType}\" can therefor not be handled.");
                }

                try
                {
                    await _messageHandlerExecutionStrategy.Execute(
                        ct => ExecuteHandler((dynamic)messageInstance, (dynamic)handler, handlerContext, ct),
                        executionContext,
                        cancellationToken);
                }
                catch (Exception exception) when (ShouldDeadLetter(exception, cancellationToken))
                {
                    failure = exception;
                }
            }, cancellationToken);

            return failure;
        }

        private Task WaitBeforeRetry(int attempt, CancellationToken cancellationToken)
        {
            var delay = _deadLetterPolicy.RetryBackoff?.Invoke(attempt) ?? TimeSpan.Zero;
            return delay > TimeSpan.Zero ? Task.Delay(delay, cancellationToken) : Task.CompletedTask;
        }

        private bool ShouldDeadLetter(Exception exception, CancellationToken cancellationToken)
        {
            return _deadLetterPolicy != null
                && !cancellationToken.IsCancellationRequested
                && !(_deadLetterPolicy.Bypass?.Invoke(exception) ?? false);
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