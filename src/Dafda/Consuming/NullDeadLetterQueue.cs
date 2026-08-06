namespace Dafda.Consuming;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A no-op <see cref="IDeadLetterQueue"/> used when no dead letter queue is
/// configured. Its presence signals that failed messages should be rethrown
/// (preserving the pre-existing behavior).
/// </summary>
internal sealed class NullDeadLetterQueue : IDeadLetterQueue
{
    public static readonly NullDeadLetterQueue Instance = new();

    private NullDeadLetterQueue()
    {
    }

    public Task Send(MessageResult message, Exception exception, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}