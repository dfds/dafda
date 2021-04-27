using System;
using System.Linq;
using Dafda.Consuming;
using Dafda.Producing;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestPayloadDescriptorFactory
    {
        [Fact]
        public void should_add_metadata()
        {
            var registry = A.OutgoingMessageRegistry
                .Register<TestProducer.Message>("foo", "bar", @event => @event.Id)
                .Build();
            var generator = new MessageIdGeneratorStub(() => "1");
            var sut = new PayloadDescriptorFactory(registry, generator);

            var message = new TestProducer.Message()
            {
                Id = "myId"
            };

            var headers = new Metadata();

            var result = sut.Create(message, headers);
            var correlationId = result.MessageHeaders.First(h => string.Equals(h.Key, "correlationId", StringComparison.InvariantCultureIgnoreCase)).Value;
            var causationId = result.MessageHeaders.First(h => string.Equals(h.Key, "causationId", StringComparison.InvariantCultureIgnoreCase)).Value;

            Assert.Equal("1", correlationId);
            Assert.Equal("1", causationId);
        }
    }
}