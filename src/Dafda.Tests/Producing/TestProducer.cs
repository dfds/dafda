using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Producing;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestProducer
    {
        [Fact]
        public void Has_expected_message_id_header_name()
        {
            Assert.Equal("messageId", Producer.MessageIdHeaderName);
        }

        [Fact]
        public void Has_expected_type_header_name()
        {
            Assert.Equal("type", Producer.TypeHeaderName);
        }

        [Fact]
        public void Message_has_expected_key()
        {
            var message = Producer.PrepareOutgoingMessage(new OutgoingMessageBuilder().WithKey("dummyKey"));

            Assert.Equal("dummyKey", message.Key);
        }

        [Fact]
        public void Message_has_expected_value()
        {
            var message = Producer.PrepareOutgoingMessage(new OutgoingMessageBuilder().WithValue("dummyMessage"));

            Assert.Equal("dummyMessage", message.Value);
        }

        [Fact]
        public void Message_header_has_expected_message_id()
        {
            var message = Producer.PrepareOutgoingMessage(new OutgoingMessageBuilder().WithMessageId("A"));

            Assert.Equal(new[] {(byte) 'A'}, message.Headers.GetLastBytes(Producer.MessageIdHeaderName));
        }

        [Fact]
        public void Message_header_has_expected_type()
        {
            var message = Producer.PrepareOutgoingMessage(new OutgoingMessageBuilder().WithType("T"));

            Assert.Equal(new[] {(byte) 'T'}, message.Headers.GetLastBytes(Producer.TypeHeaderName));
        }

        [Fact]
        public async Task Can_produce_message()
        {
            var spy = new KafkaProducerSpy();

            var builder = new OutgoingMessageFactoryBuilder()
                .With(new OutgoingMessageRegistryBuilder().Register<DomainEvent>("foo", "bar", @event => @event.AggregateId));

            var sut = new Producer(spy, builder);

            await sut.Produce(new DomainEvent
            {
                AggregateId = "dummyId"
            });

            Assert.Equal("foo", spy.LastTopic);
            Assert.Equal("dummyId", spy.LastMessage.Key);
        }

        public class DomainEvent
        {
            public string AggregateId { get; set; }
        }
    }
}