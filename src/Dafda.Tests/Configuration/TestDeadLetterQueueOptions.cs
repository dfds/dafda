namespace Dafda.Tests.Configuration;

using System;
using Dafda.Configuration;
using Xunit;

public class TestDeadLetterQueueOptions
{
    [Fact]
    public void has_no_retry_backoff_by_default()
    {
        var sut = new DeadLetterQueueOptions("dlq");

        Assert.Null(sut.RetryBackoff);
    }

    [Fact]
    public void fixed_retry_backoff_returns_the_same_delay_for_every_attempt()
    {
        var sut = new DeadLetterQueueOptions("dlq")
            .WithRetryBackoff(TimeSpan.FromSeconds(3));

        Assert.Equal(TimeSpan.FromSeconds(3), sut.RetryBackoff(1));
        Assert.Equal(TimeSpan.FromSeconds(3), sut.RetryBackoff(2));
        Assert.Equal(TimeSpan.FromSeconds(3), sut.RetryBackoff(7));
    }

    [Fact]
    public void exponential_retry_backoff_doubles_by_default()
    {
        var sut = new DeadLetterQueueOptions("dlq")
            .WithExponentialRetryBackoff(TimeSpan.FromSeconds(1));

        Assert.Equal(TimeSpan.FromSeconds(1), sut.RetryBackoff(1));
        Assert.Equal(TimeSpan.FromSeconds(2), sut.RetryBackoff(2));
        Assert.Equal(TimeSpan.FromSeconds(4), sut.RetryBackoff(3));
        Assert.Equal(TimeSpan.FromSeconds(8), sut.RetryBackoff(4));
    }

    [Fact]
    public void exponential_retry_backoff_uses_the_supplied_factor()
    {
        var sut = new DeadLetterQueueOptions("dlq")
            .WithExponentialRetryBackoff(TimeSpan.FromSeconds(1), factor: 3);

        Assert.Equal(TimeSpan.FromSeconds(1), sut.RetryBackoff(1));
        Assert.Equal(TimeSpan.FromSeconds(3), sut.RetryBackoff(2));
        Assert.Equal(TimeSpan.FromSeconds(9), sut.RetryBackoff(3));
    }

    [Fact]
    public void exponential_retry_backoff_is_capped_by_max_delay()
    {
        var sut = new DeadLetterQueueOptions("dlq")
            .WithExponentialRetryBackoff(TimeSpan.FromSeconds(1), maxDelay: TimeSpan.FromSeconds(5));

        Assert.Equal(TimeSpan.FromSeconds(1), sut.RetryBackoff(1));
        Assert.Equal(TimeSpan.FromSeconds(2), sut.RetryBackoff(2));
        Assert.Equal(TimeSpan.FromSeconds(4), sut.RetryBackoff(3));
        Assert.Equal(TimeSpan.FromSeconds(5), sut.RetryBackoff(4));
        Assert.Equal(TimeSpan.FromSeconds(5), sut.RetryBackoff(100));
    }

    [Fact]
    public void throws_when_fixed_retry_backoff_delay_is_negative()
    {
        var sut = new DeadLetterQueueOptions("dlq");

        Assert.Throws<InvalidConfigurationException>(() => sut.WithRetryBackoff(TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void throws_when_exponential_retry_backoff_initial_delay_is_negative()
    {
        var sut = new DeadLetterQueueOptions("dlq");

        Assert.Throws<InvalidConfigurationException>(() => sut.WithExponentialRetryBackoff(TimeSpan.FromSeconds(-1)));
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(-1d)]
    public void throws_when_exponential_retry_backoff_factor_is_not_positive(double factor)
    {
        var sut = new DeadLetterQueueOptions("dlq");

        Assert.Throws<InvalidConfigurationException>(() => sut.WithExponentialRetryBackoff(TimeSpan.FromSeconds(1), factor));
    }

    [Fact]
    public void throws_when_exponential_retry_backoff_max_delay_is_negative()
    {
        var sut = new DeadLetterQueueOptions("dlq");

        Assert.Throws<InvalidConfigurationException>(
            () => sut.WithExponentialRetryBackoff(TimeSpan.FromSeconds(1), maxDelay: TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void throws_when_fixed_retry_backoff_delay_exceeds_the_supported_maximum()
    {
        var sut = new DeadLetterQueueOptions("dlq");

        Assert.Throws<InvalidConfigurationException>(
            () => sut.WithRetryBackoff(DeadLetterQueueOptions.MaxSupportedRetryDelay + TimeSpan.FromMilliseconds(1)));
    }

    [Fact]
    public void throws_when_exponential_retry_backoff_initial_delay_exceeds_the_supported_maximum()
    {
        var sut = new DeadLetterQueueOptions("dlq");

        Assert.Throws<InvalidConfigurationException>(
            () => sut.WithExponentialRetryBackoff(DeadLetterQueueOptions.MaxSupportedRetryDelay + TimeSpan.FromMilliseconds(1)));
    }

    [Fact]
    public void throws_when_exponential_retry_backoff_max_delay_exceeds_the_supported_maximum()
    {
        var sut = new DeadLetterQueueOptions("dlq");

        Assert.Throws<InvalidConfigurationException>(
            () => sut.WithExponentialRetryBackoff(
                TimeSpan.FromSeconds(1),
                maxDelay: DeadLetterQueueOptions.MaxSupportedRetryDelay + TimeSpan.FromMilliseconds(1)));
    }

    [Fact]
    public void exponential_retry_backoff_clamps_overflow_to_the_supported_maximum()
    {
        var sut = new DeadLetterQueueOptions("dlq")
            .WithExponentialRetryBackoff(TimeSpan.FromSeconds(1));

        Assert.Equal(DeadLetterQueueOptions.MaxSupportedRetryDelay, sut.RetryBackoff(64));
        Assert.Equal(DeadLetterQueueOptions.MaxSupportedRetryDelay, sut.RetryBackoff(int.MaxValue));
    }

    [Fact]
    public void exponential_retry_backoff_prefers_an_explicit_max_delay_over_the_supported_maximum()
    {
        var sut = new DeadLetterQueueOptions("dlq")
            .WithExponentialRetryBackoff(TimeSpan.FromSeconds(1), maxDelay: TimeSpan.FromSeconds(30));

        Assert.Equal(TimeSpan.FromSeconds(30), sut.RetryBackoff(64));
        Assert.Equal(TimeSpan.FromSeconds(30), sut.RetryBackoff(int.MaxValue));
    }

    [Fact]
    public void exponential_retry_backoff_returns_zero_for_a_zero_initial_delay()
    {
        var sut = new DeadLetterQueueOptions("dlq")
            .WithExponentialRetryBackoff(TimeSpan.Zero, factor: 1000);

        Assert.Equal(TimeSpan.Zero, sut.RetryBackoff(1));
        Assert.Equal(TimeSpan.Zero, sut.RetryBackoff(64));
        Assert.Equal(TimeSpan.Zero, sut.RetryBackoff(int.MaxValue));
    }

    [Fact]
    public void throws_when_max_retries_is_negative()
    {
        var sut = new DeadLetterQueueOptions("dlq");

        Assert.Throws<InvalidConfigurationException>(() => sut.WithMaxRetries(-1));
    }
}
