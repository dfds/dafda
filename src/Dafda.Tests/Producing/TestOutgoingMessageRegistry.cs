using Dafda.Producing;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestOutgoingMessageRegistry
    {
        [Fact]
        public void Test1()
        {
            var sut = new OutgoingMessageRegistry();
            sut.Register<DomainEvent>("foo", "bar", @event => @event.AggregateId);

            var domainEvent = new DomainEvent("baz");
            var registration = sut.GetRegistration(domainEvent);

            Assert.Equal("foo", registration.Topic);
            Assert.Equal("bar", registration.Type);
            Assert.Equal("baz", registration.KeySelector(domainEvent));
        }

        public class DomainEvent
        {
            public DomainEvent(string aggregateId)
            {
                AggregateId = aggregateId;
            }

            public string AggregateId { get; }
        }
    }
}