using System;
using System.Threading.Tasks;

namespace Dafda.Consuming;

/// <summary>
/// Defines a strategy for executing consumer actions asynchronously.
/// </summary>
/// <remarks>Implementations of this interface can provide custom execution logic, such as retry policies or error
/// handling, for actions that are executed asynchronously.</remarks>
public interface IConsumerExecutionStrategy
{
    /// <summary>
    /// Executes the specified asynchronous action.
    /// </summary>
    /// <param name="action">The asynchronous action to execute. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Execute(Func<Task> action);
}
