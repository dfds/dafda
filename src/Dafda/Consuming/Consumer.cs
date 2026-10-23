namespace Dafda.Consuming;

using System;
using System.Threading;
using System.Threading.Tasks;
using Diagnostics;
using Interfaces;
using MessageFilters;

internal class Consumer(
    MessageHandlerRegistry messageHandlerRegistry,
    IHandlerUnitOfWorkFactory unitOfWorkFactory,
    IConsumerScopeFactory consumerScopeFactory,
    IUnconfiguredMessageHandlingStrategy fallbackHandler,
    MessageFilter messageFilter,
    IMessageHandlerExecutionStrategy messageHandlerExecutionStrategy,
    bool isAutoCommitEnabled = false,
    IDeadLetterQueue deadLetterQueue = null,
    int maxRetries = 0,
    Func<Exception, bool> deadLetterQueueBypass = null,
    Func<int, TimeSpan> retryBackoff = null)
    : IConsumer, IDisposable
{
    private readonly LocalMessageDispatcher _localMessageDispatcher = new(
        messageHandlerRegistry,
        unitOfWorkFactory,
        fallbackHandler,
        messageHandlerExecutionStrategy);

    private readonly IDeadLetterQueue _deadLetterQueue = deadLetterQueue;

    public async Task ConsumeAll(CancellationToken cancellationToken)
    {
        using var consumerScope = consumerScopeFactory.CreateConsumerScope();
        while (!cancellationToken.IsCancellationRequested)
        {
            await ProcessNextMessage(consumerScope, cancellationToken);
        }
    }

    public async Task ConsumeSingle(CancellationToken cancellationToken)
    {
        using var consumerScope = consumerScopeFactory.CreateConsumerScope();
        await ProcessNextMessage(consumerScope, cancellationToken);
    }

    private async Task ProcessNextMessage(ConsumerScope consumerScope, CancellationToken cancellationToken)
    {
        var messageResult = await consumerScope.GetNext(cancellationToken);
        using var activity = DafdaActivitySource.StartReceivingActivity(messageResult);

        if (messageFilter.CanAcceptMessage(messageResult))
        {
            await Dispatch(messageResult, cancellationToken);
        }

        if (!isAutoCommitEnabled)
        {
            await messageResult.Commit(cancellationToken);
        }
    }

    private async Task Dispatch(MessageResult messageResult, CancellationToken cancellationToken)
    {
        var attempt = 0;

        while (true)
        {
            try
            {
                await _localMessageDispatcher.Dispatch(messageResult, cancellationToken);
                return;
            }
            catch (Exception exception) when (ShouldDeadLetter(exception, cancellationToken))
            {
                if (attempt < maxRetries)
                {
                    attempt++;

                    var delay = GetRetryDelay(attempt);
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

    private TimeSpan GetRetryDelay(int attempt)
    {
        return retryBackoff == null ? TimeSpan.Zero : retryBackoff(attempt);
    }

    private bool ShouldDeadLetter(Exception exception, CancellationToken cancellationToken)
    {
        return _deadLetterQueue != null
            && !cancellationToken.IsCancellationRequested
            && !ShouldBypassDeadLetterQueue(exception)
            && !IsDispatchConfigurationError(exception);
    }

    private bool ShouldBypassDeadLetterQueue(Exception exception)
    {
        return deadLetterQueueBypass != null && deadLetterQueueBypass(exception);
    }

    // Thrown by Dafda before a handler runs, when no handler can be found or created. No handler
    // failed, so these are configuration errors and are not dead-lettered.
    private static bool IsDispatchConfigurationError(Exception exception)
    {
        return exception is MissingMessageHandlerRegistrationException
            or InvalidMessageHandlerException
            or UnableToResolveUnitOfWorkForHandlerException;
    }

    public void Dispose()
    {
        (_deadLetterQueue as IDisposable)?.Dispose();
    }
}