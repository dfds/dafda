using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Consuming;

/// <summary>
/// Represents a direct execution strategy for message handler actions that executes the provided asynchronous action.
/// This is the default implementation used when no custom execution strategy is specified.
/// </summary>
public class DirectMessageHandlerExecutionStrategy : IMessageHandlerExecutionStrategy
{
    /// <summary>
    /// Executes the specified asynchronous action.
    /// </summary>
    /// <param name="action">The asynchronous action to execute. Cannot be null.</param>
    /// <param name="executionContext">The execution context containing the message instance and metadata.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task Execute(Func<CancellationToken, Task> action, MessageExecutionContext executionContext, CancellationToken cancellationToken)
    {
        await action(cancellationToken);
    }
}
