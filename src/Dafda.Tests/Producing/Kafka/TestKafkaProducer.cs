using System.Threading.Tasks;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Producing.Kafka
{
    public class TestKafkaProducer
    {
        [Fact]
        public async Task produces_to_expected_topic()
        {
            var spy = new KafkaProducerSpy();

            var expected = "foo topic name";

            var payloadStub = new PayloadDescriptorBuilder()
                .WithTopicName(expected)
                .Build();

            await spy.Produce(payloadStub);

            Assert.Equal(expected, spy.Topic);
        }

        [Fact]
        public async Task produces_message_with_expected_key()
        {
            var spy = new KafkaProducerSpy();

            var expected = "foo partition key";
            
            var payloadStub = new PayloadDescriptorBuilder()
                .WithPartitionKey(expected)
                .Build();

            await spy.Produce(payloadStub);

            Assert.Equal(expected, spy.Key);
        }

        [Fact]
        public async Task produces_message_with_expected_value()
        {
            var expected = "foo value 123";
            var spy = new KafkaProducerSpy(new PayloadSerializerStub(expected));

            var payloadStub = new PayloadDescriptorBuilder().Build();
            await spy.Produce(payloadStub);

            Assert.Equal(expected, spy.Value);
        }
    }
}