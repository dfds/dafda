using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Producing;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestBus
    {
        [Fact]
        public async Task Can_produce_message()
        {
            var spy = new ProducerSpy();
            var configurationBuilder = new ProducerConfigurationBuilder();
            configurationBuilder.WithBootstrapServers("foo");
            var configuration = configurationBuilder.Build();
            
            var sut = new Bus(spy, configuration);

            await sut.Publish(new DomainEvent
            {
                AggregateId = "dummyId"
            });

            Assert.Equal("foo", spy.LastOutgoingMessage.Topic);
            Assert.Equal("dummyId", spy.LastOutgoingMessage.Key);
        }

        [Fact]
        public async Task Can_produce_message_with_annotation()
        {
            var spy = new ProducerSpy();
            var configurationBuilder = new ProducerConfigurationBuilder();
            configurationBuilder.WithBootstrapServers("foo");
            var registry = new OutgoingMessageRegistry();
            registry.Register<UnannotatedDomainEvent>("foo", "bar", x => x.AggregateId);
            configurationBuilder.WithOutgoingMessageRegistry(registry);
            var configuration = configurationBuilder.Build();
            
            var sut = new Bus(spy, configuration);

            await sut.Publish(new UnannotatedDomainEvent
            {
                AggregateId = "dummyId"
            });

            Assert.Equal("foo", spy.LastOutgoingMessage.Topic);
            Assert.Equal("dummyId", spy.LastOutgoingMessage.Key);
        }

        [Message("foo", "bar")]
        public class DomainEvent : IMessage
        {
            public string AggregateId { get; set; }
        }
        
        public class UnannotatedDomainEvent
        {
            public string AggregateId { get; set; }
        }

    }
}