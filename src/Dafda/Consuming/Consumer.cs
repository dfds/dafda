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
    int maxRetries = 0)
    : IConsumer
{
    private readonly LocalMessageDispatcher _localMessageDispatcher = new(
        messageHandlerRegistry,
        unitOfWorkFactory,
        fallbackHandler,
        messageHandlerExecutionStrategy);

    private readonly IDeadLetterQueue _deadLetterQueue = deadLetterQueue ?? NullDeadLetterQueue.Instance;

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
        var deadLetterQueueEnabled = _deadLetterQueue is not NullDeadLetterQueue;
        var attempt = 0;

        while (true)
        {
            try
            {
                await _localMessageDispatcher.Dispatch(messageResult, cancellationToken);
                return;
            }
            catch (Exception exception) when (deadLetterQueueEnabled)
            {
                if (attempt++ < maxRetries)
                {
                    continue;
                }

                await _deadLetterQueue.Send(messageResult, exception, cancellationToken);
                return;
            }
        }
    }
}