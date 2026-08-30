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
    /// A predicate matching exceptions that should bypass the dead letter queue.
    /// When an exception matches, it is rethrown (crashing the consumer) instead
    /// of being retried or forwarded to the dead letter queue. Returns <c>null</c>
    /// when no bypass has been configured.
    /// </summary>
    internal Func<Exception, bool> BypassPredicate
    {
        get
        {
            if (_bypassPredicates.Count == 0)
            {
                return null;
            }

            var snapshot = _bypassPredicates.ToArray();
            return exception => snapshot.Any(predicate => predicate(exception));
        }
    }

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