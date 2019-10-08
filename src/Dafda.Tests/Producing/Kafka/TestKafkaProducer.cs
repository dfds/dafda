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
        public void Has_expected_message_id_header_name()
        {
            Assert.Equal("messageId", KafkaProducer.MessageIdHeaderName);
        }

        [Fact]
        public void Has_expected_type_header_name()
        {
            Assert.Equal("type", KafkaProducer.TypeHeaderName);
        }

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

        [Fact]
        public void Message_header_has_expected_message_id()
        {
            var message = KafkaProducer.PrepareOutgoingMessage(EmptyOutgoingMessage.WithMessageId("A"));

            Assert.Equal(new[] {(byte) 'A'}, message.Headers.GetLastBytes(KafkaProducer.MessageIdHeaderName));
        }

        [Fact]
        public void Message_header_has_expected_type()
        {
            var message = KafkaProducer.PrepareOutgoingMessage(EmptyOutgoingMessage.WithType("T"));

            Assert.Equal(new[] {(byte) 'T'}, message.Headers.GetLastBytes(KafkaProducer.TypeHeaderName));
        }
    }
}