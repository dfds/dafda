namespace Dafda.Configuration;

/// <summary>
/// Fluent options for configuring a dead letter queue on a consumer.
/// Returned by <see cref="ConsumerOptions.WithDeadLetterQueue"/>.
/// </summary>
public sealed class DeadLetterQueueOptions
{
    internal DeadLetterQueueOptions(string topicName)
    {
        TopicName = topicName;
    }

    /// <summary>
    /// The explicit dead letter topic name. When <c>null</c>, the topic is
    /// derived from the source topic of the failed message.
    /// </summary>
    internal string TopicName { get; }

    /// <summary>
    /// The number of additional delivery attempts after the first failure,
    /// before the message is forwarded to the dead letter queue.
    /// </summary>
    internal int MaxRetries { get; private set; }

    /// <summary>
    /// Set the maximum number of retries (additional attempts after the first
    /// delivery) before a failing message is sent to the dead letter queue.
    /// </summary>
    /// <param name="maxRetries">The number of retries. Must be zero or greater.</param>
    public DeadLetterQueueOptions WithMaxRetries(int maxRetries)
    {
        if (maxRetries < 0)
        {
            throw new InvalidConfigurationException("The number of retries for a dead letter queue cannot be negative.");
        }

        MaxRetries = maxRetries;
        return this;
    }
}