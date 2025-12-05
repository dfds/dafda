using Dafda.Consuming;
using System;
using System.Threading.Tasks;

namespace Dafda.Tests.TestDoubles;
public class ConsumerExecutionStrategyStub(IInnerConsumerExecutionStrategy inner) : IConsumerExecutionStrategy
{
    public async Task Execute(Func<Task> action)
    {
        await inner.InnerExecute(action);
    }
}

public interface IInnerConsumerExecutionStrategy
{
    Task InnerExecute(Func<Task> action);
}