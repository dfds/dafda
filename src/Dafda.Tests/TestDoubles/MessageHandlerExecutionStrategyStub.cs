using Dafda.Consuming;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Tests.TestDoubles;
public class MessageHandlerExecutionStrategyStub(IInnerMessageHandlerExecutionStrategy inner) : IMessageHandlerExecutionStrategy
{
    public async Task Execute(Func<CancellationToken, Task> action, MessageExecutionContext executionContext, CancellationToken cancellationToken)
    {
        await inner.InnerExecute(action);
    }
}

public interface IInnerMessageHandlerExecutionStrategy
{
    Task InnerExecute(Func<CancellationToken, Task> action);
}