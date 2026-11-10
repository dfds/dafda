using System;

namespace Dafda.Consuming;

/// <summary>
/// How failed messages are retried and dead-lettered. A consumer without a dead
/// letter queue has no policy (<c>null</c>).
/// </summary>
internal sealed record DeadLetterPolicy(
    IDeadLetterQueue Queue,
    int MaxRetries = 0,
    Func<Exception, bool> Bypass = null,
    Func<int, TimeSpan> RetryBackoff = null);
