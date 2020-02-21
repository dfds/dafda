using Dafda.Producing;
using Dafda.Producing.Kafka;
using Xunit;

namespace Dafda.Tests.Producing.Kafka
{
    public class TestKafkaProducer
    {
        private static OutgoingMessageBuilder EmptyOutgoingMessage =>
            new OutgoingMessageBuilder()
                .WithTopic("")
                .WithMessageId("")
                .WithKey("")
                .WithValue("")
                .WithType("");

        [Fact]
        public void Message_has_expected_key()
        {
            var message = KafkaProducer.PrepareOutgoingMessage(EmptyOutgoingMessage.WithKey("dummyKey"));

            Assert.Equal("dummyKey", message.Key);
        }

        [Fact]
        public void Message_has_expected_value()
        {
            var message = KafkaProducer.PrepareOutgoingMessage(EmptyOutgoingMessage.WithValue("dummyMessage"));

            Assert.Equal("dummyMessage", message.Value);
        }
    }
}