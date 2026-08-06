namespace Dafda.Consuming;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Forwards a message that could not be handled (after retries are exhausted)
/// to a dead letter queue, so the consumer can move past the poison message.
/// </summary>
internal interface IDeadLetterQueue
{
    /// <summary>
    /// Publish the failed <paramref name="message"/> to the dead letter queue.
    /// </summary>
    Task Send(MessageResult message, Exception exception, CancellationToken cancellationToken);
}