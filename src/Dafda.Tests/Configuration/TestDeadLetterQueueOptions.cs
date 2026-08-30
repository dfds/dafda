namespace Dafda.Tests.Configuration;

using System;
using Dafda.Configuration;
using Xunit;

public class TestDeadLetterQueueOptions
{
    [Fact]
    public void has_no_bypass_predicate_by_default()
    {
        var sut = new DeadLetterQueueOptions("dlq");

        Assert.Null(sut.BypassPredicate);
    }

    [Fact]
    public void bypass_predicate_matches_the_registered_exception_type()
    {
        var sut = new DeadLetterQueueOptions("dlq")
            .BypassFor<InvalidOperationException>();

        Assert.True(sut.BypassPredicate(new InvalidOperationException()));
        Assert.False(sut.BypassPredicate(new FormatException()));
    }

    [Fact]
    public void bypass_predicate_matches_types_derived_from_the_registered_exception_type()
    {
        var sut = new DeadLetterQueueOptions("dlq")
            .BypassFor<ArgumentException>();

        Assert.True(sut.BypassPredicate(new ArgumentNullException()));
    }

    [Fact]
    public void bypass_predicate_matches_any_of_the_registered_predicates()
    {
        var sut = new DeadLetterQueueOptions("dlq")
            .BypassFor<InvalidOperationException>()
            .BypassWhen(exception => exception is FormatException);

        Assert.True(sut.BypassPredicate(new InvalidOperationException()));
        Assert.True(sut.BypassPredicate(new FormatException()));
        Assert.False(sut.BypassPredicate(new NotSupportedException()));
    }

    [Fact]
    public void throws_when_the_bypass_predicate_is_null()
    {
        var sut = new DeadLetterQueueOptions("dlq");

        Assert.Throws<InvalidConfigurationException>(() => sut.BypassWhen(null));
    }

    [Fact]
    public void bypass_predicate_is_not_affected_by_predicates_registered_afterwards()
    {
        var sut = new DeadLetterQueueOptions("dlq")
            .BypassFor<InvalidOperationException>();

        var predicate = sut.BypassPredicate;

        sut.BypassFor<FormatException>();

        Assert.False(predicate(new FormatException()));
        Assert.True(sut.BypassPredicate(new FormatException()));
    }
}
