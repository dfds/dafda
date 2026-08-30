namespace Dafda.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Fluent options for configuring a dead letter queue on a consumer.
/// Returned by <see cref="ConsumerOptions.WithDeadLetterQueue"/>.
/// </summary>
public sealed class DeadLetterQueueOptions
{
    private readonly List<Func<Exception, bool>> _bypassPredicates = new();

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

    /// <summary>
    /// Maps a retry attempt number (the first retry is attempt <c>1</c>) to the delay
    /// awaited before that attempt is made. Returns <c>null</c> when no backoff has been
    /// configured, in which case retries happen without any delay.
    /// </summary>
    internal Func<int, TimeSpan> RetryBackoff { get; private set; }

    /// <summary>
    /// Wait a fixed <paramref name="delay"/> before each retry attempt.
    /// </summary>
    /// <param name="delay">The delay awaited before every retry attempt. Must be zero or greater.</param>
    public DeadLetterQueueOptions WithRetryBackoff(TimeSpan delay)
    {
        if (delay < TimeSpan.Zero)
        {
            throw new InvalidConfigurationException("The retry backoff delay for a dead letter queue cannot be negative.");
        }

        RetryBackoff = _ => delay;
        return this;
    }

    /// <summary>
    /// Wait an exponentially increasing delay before each retry attempt. The delay before
    /// retry attempt <c>n</c> (the first retry being attempt <c>1</c>) is
    /// <paramref name="initialDelay"/> multiplied by <paramref name="factor"/> raised to the
    /// power of <c>n - 1</c>, optionally capped by <paramref name="maxDelay"/>.
    /// </summary>
    /// <param name="initialDelay">The delay awaited before the first retry attempt. Must be zero or greater.</param>
    /// <param name="factor">The multiplier applied for each subsequent attempt. Must be greater than zero.</param>
    /// <param name="maxDelay">An optional upper bound for the computed delay. Must be zero or greater when supplied.</param>
    public DeadLetterQueueOptions WithExponentialRetryBackoff(TimeSpan initialDelay, double factor = 2.0, TimeSpan? maxDelay = null)
    {
        if (initialDelay < TimeSpan.Zero)
        {
            throw new InvalidConfigurationException("The retry backoff delay for a dead letter queue cannot be negative.");
        }

        if (factor <= 0)
        {
            throw new InvalidConfigurationException("The retry backoff factor for a dead letter queue must be greater than zero.");
        }

        if (maxDelay.HasValue && maxDelay.Value < TimeSpan.Zero)
        {
            throw new InvalidConfigurationException("The maximum retry backoff delay for a dead letter queue cannot be negative.");
        }

        RetryBackoff = attempt => CalculateExponentialDelay(initialDelay, factor, maxDelay, attempt);
        return this;
    }

    private static TimeSpan CalculateExponentialDelay(TimeSpan initialDelay, double factor, TimeSpan? maxDelay, int attempt)
    {
        var exponent = attempt < 1 ? 0 : attempt - 1;
        var ticks = initialDelay.Ticks * Math.Pow(factor, exponent);

        var delay = ticks >= TimeSpan.MaxValue.Ticks
            ? TimeSpan.MaxValue
            : TimeSpan.FromTicks((long)ticks);

        if (maxDelay.HasValue && delay > maxDelay.Value)
        {
            return maxDelay.Value;
        }

        return delay;
    }

    /// <summary>
    /// A predicate matching exceptions that should bypass the dead letter queue.
    /// When an exception matches, it is rethrown (crashing the consumer) instead
    /// of being retried or forwarded to the dead letter queue. Returns <c>null</c>
    /// when no bypass has been configured.
    /// </summary>
    internal Func<Exception, bool> BypassPredicate =>
        _bypassPredicates.Count == 0
            ? null
            : exception => _bypassPredicates.Any(predicate => predicate(exception));

    /// <summary>
    /// Bypass the dead letter queue for the specified exception type (and any
    /// derived types). When a message handler throws a matching exception, it is
    /// rethrown so the consumer crashes instead of dead-lettering the message.
    /// </summary>
    /// <typeparam name="TException">The exception type to bypass the dead letter queue for.</typeparam>
    public DeadLetterQueueOptions BypassFor<TException>() where TException : Exception
    {
        _bypassPredicates.Add(exception => exception is TException);
        return this;
    }

    /// <summary>
    /// Bypass the dead letter queue for exceptions matching the supplied
    /// <paramref name="predicate"/>. When it returns <c>true</c>, the exception is
    /// rethrown so the consumer crashes instead of dead-lettering the message.
    /// </summary>
    /// <param name="predicate">Evaluates a thrown exception and returns <c>true</c> to bypass the dead letter queue.</param>
    public DeadLetterQueueOptions BypassWhen(Func<Exception, bool> predicate)
    {
        if (predicate == null)
        {
            throw new InvalidConfigurationException("The dead letter queue bypass predicate cannot be null.");
        }

        _bypassPredicates.Add(predicate);
        return this;
    }
}