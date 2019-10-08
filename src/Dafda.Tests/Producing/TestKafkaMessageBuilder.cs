using Dafda.Producing;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestKafkaMessageBuilder
    {
        [Fact]
        public void Message_has_expected_key()
        {
            var message = new KafkaMessageBuilder()
                .WithKey("dummyKey")
                .Build();

            Assert.Equal("dummyKey", message.Key);
        }

        [Fact]
        public void Message_has_expected_value()
        {
            var message = new KafkaMessageBuilder()
                .WithValue("dummyMessage")
                .Build();

            Assert.Equal("dummyMessage", message.Value);
        }

        [Fact]
        public void Message_has_expected_header()
        {
            const char chr = 'A';

            var message = new KafkaMessageBuilder()
                .WithHeader("header", chr.ToString())
                .Build();

            Assert.Equal(new[] {(byte) chr}, message.Headers.GetLastBytes("header"));
        }
    }
}