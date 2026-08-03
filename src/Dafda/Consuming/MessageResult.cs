namespace Dafda.Consuming;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Object that contains message when consumed from Kafka.
/// To be used for message handling prior to dispatching to the handlers.
/// </summary>
public class MessageResult
{
    private static readonly Func<CancellationToken, Task> EmptyCommitAction = (_) => Task.CompletedTask;
    private readonly Func<CancellationToken, Task> _onCommit;

    /// <summary>
    /// Resulting Message containing Transport Level Message
    /// </summary>
    public MessageResult(TransportLevelMessage message, Func<CancellationToken, Task> onCommit = null)
    {
        Message = message;
        _onCommit = onCommit ?? EmptyCommitAction;
    }

    internal string Topic { get; set; }

    /// <summary>
    /// The raw, unparsed message value as received from Kafka. Used when
    /// forwarding a failed message to a dead letter queue.
    /// </summary>
    public string RawMessage { get; internal set; }

    internal int Partition { get; set; }

    internal string PartitionKey { get; set; }

    internal string ClientId { get; set; }

    internal string GroupId { get; set; }

    /// <summary>
    /// Transmitted message consumed from Kafka
    /// </summary>
    public TransportLevelMessage Message { get; }

    /// <summary>
    /// Commit message to handlers
    /// </summary>
    public async Task Commit(CancellationToken cancellationToken)
    {
        await _onCommit(cancellationToken);
    }
}