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
            sut.Register<Message>("foo", "bar", @event => @event.Id);

            var domainEvent = new Message("baz");
            var registration = sut.GetRegistration(domainEvent);

            Assert.Equal("foo", registration.Topic);
            Assert.Equal("bar", registration.Type);
            Assert.Equal("baz", registration.KeySelector(domainEvent));
        }

        public class Message
        {
            public Message(string id)
            {
                Id = id;
            }

            public string Id { get; }
        }
    }
}