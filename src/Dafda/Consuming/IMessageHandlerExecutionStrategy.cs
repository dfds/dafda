using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Consuming;

/// <summary>
/// Defines a strategy for executing message handler actions asynchronously.
/// </summary>
/// <remarks>Implementations of this interface can provide custom execution logic, such as retry policies or error
/// handling, for actions that are executed asynchronously.</remarks>
public interface IMessageHandlerExecutionStrategy
{
    /// <summary>
    /// Executes the specified asynchronous action.
    /// </summary>
    /// <param name="action">The asynchronous action to execute.</param>
    /// <param name="executionContext">The execution context containing the message instance and metadata.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Execute(Func<CancellationToken, Task> action, MessageExecutionContext executionContext, CancellationToken cancellationToken);
}
