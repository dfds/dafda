using System;
using System.Threading.Tasks;

namespace Dafda.Consuming;

/// <summary>
/// Represents a simple execution strategy for consumer actions that executes the provided asynchronous action.
/// </summary>
public class SimpleConsumerExecutionStrategy : IConsumerExecutionStrategy
{
    /// <summary>
    /// Executes the specified asynchronous action.
    /// </summary>
    /// <param name="action">The asynchronous action to execute. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task Execute(Func<Task> action)
    {
        await action();
    }
}
