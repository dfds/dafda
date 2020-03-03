using Dafda.Producing;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestProducerFactory
    {
        [Fact]
        public void returns_expected_when_nothing_has_been_registered()
        {
            var sut = new ProducerFactoryBuilder().Build();
            var result = sut.Get("foo", NullLoggerFactory.Instance);

            Assert.Null(result);
        }

        [Fact]
        public void returns_expected_when_getting_by_a_known_name()
        {
            var producerConfigurationStub = A.ValidProducerConfiguration.Build();
            var outgoingMessageRegistryStub = new OutgoingMessageRegistry();

            var sut = new ProducerFactoryBuilder().Build();
            sut.ConfigureProducer("foo", producerConfigurationStub, outgoingMessageRegistryStub);

            var result = sut.Get("foo", NullLoggerFactory.Instance);

            Assert.IsType<Producer>(result);
            Assert.NotNull(result);
        }

        [Fact]
        public void returns_expected_when_getting_by_an_unknown_name()
        {
            var producerConfigurationStub = A.ValidProducerConfiguration.Build();
            var outgoingMessageRegistryStub = new OutgoingMessageRegistry();

            var sut = new ProducerFactoryBuilder().Build();
            sut.ConfigureProducer("foo", producerConfigurationStub, outgoingMessageRegistryStub);

            var result = sut.Get("bar", NullLoggerFactory.Instance);

            Assert.Null(result);
        }

        [Fact]
        public void throws_expected_when_adding_multiple_producers_with_same_name()
        {
            var sut = new ProducerFactoryBuilder().Build();

            sut.ConfigureProducer(
                producerName: "foo",
                configuration: A.ValidProducerConfiguration.Build(),
                outgoingMessageRegistry: new OutgoingMessageRegistry()
            );

            Assert.Throws<ProducerFactoryException>(() => sut.ConfigureProducer(
                producerName: "foo",
                configuration: A.ValidProducerConfiguration.Build(),
                outgoingMessageRegistry: new OutgoingMessageRegistry()
            ));
        }

        [Fact]
        public void when_disposing_the_factory_all_kafka_producers_are_also_disposed()
        {
            var spy = new KafkaProducerSpy();

            using (var sut = new ProducerFactoryBuilder().Build())
            {
                var producerConfiguration = A.ValidProducerConfiguration
                    .WithKafkaProducerFactory(_ => spy)
                    .Build();

                sut.ConfigureProducer(
                    producerName: "foo",
                    configuration: producerConfiguration,
                    outgoingMessageRegistry: new OutgoingMessageRegistry()
                );

                sut.Get("foo", NullLoggerFactory.Instance);
            }

            Assert.True(spy.WasDisposed);
        }
    }
}