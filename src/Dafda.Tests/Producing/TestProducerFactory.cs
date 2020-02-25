using System;
using Dafda.Configuration;
using Dafda.Producing;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestProducerFactory
    {
        [Fact]
        public void returns_expected_when_nothing_has_been_registered()
        {
            var sut = new ProducerFactoryBuilder().Build();
            var result = sut.Get("foo");

            Assert.Null(result);
        }

        [Fact]
        public void returns_expected_when_getting_by_a_known_name()
        {
            var producerConfigurationStub = new ProducerConfigurationBuilder().Build();
            var outgoingMessageRegistryStub = new OutgoingMessageRegistry();

            var sut = new ProducerFactoryBuilder().Build();
            sut.ConfigureProducer("foo", producerConfigurationStub, outgoingMessageRegistryStub);

            var result = sut.Get("foo");

            Assert.IsType<Producer>(result);
            Assert.NotNull(result);
        }

        [Fact]
        public void returns_expected_when_getting_by_an_unknown_name()
        {
            var producerConfigurationStub = new ProducerConfigurationBuilder().Build();
            var outgoingMessageRegistryStub = new OutgoingMessageRegistry();

            var sut = new ProducerFactoryBuilder().Build();
            sut.ConfigureProducer("foo", producerConfigurationStub, outgoingMessageRegistryStub);

            var result = sut.Get("bar");

            Assert.Null(result);
        }

        [Fact]
        public void throws_expected_when_adding_multiple_producers_with_same_name()
        {
            var sut = new ProducerFactoryBuilder().Build();

            sut.ConfigureProducer(
                producerName: "foo", 
                configuration: new ProducerConfigurationBuilder().Build(),
                outgoingMessageRegistry: new OutgoingMessageRegistry()
            );

            Assert.Throws<ProducerFactoryException>(() => sut.ConfigureProducer(
                producerName: "foo",
                configuration: new ProducerConfigurationBuilder().Build(),
                outgoingMessageRegistry: new OutgoingMessageRegistry()
            ));
        }

        [Fact]
        public void when_disposing_the_factory_all_kafka_producers_are_also_disposed()
        {
            var spy = new KafkaProducerSpy();

            using (var sut = new ProducerFactoryBuilder().Build())
            {
                var producerConfiguration = new ProducerConfigurationBuilder()
                    .WithKafkaProducerFactory(() => spy)
                    .Build();

                sut.ConfigureProducer(
                    producerName: "foo",
                    configuration: producerConfiguration,
                    outgoingMessageRegistry: new OutgoingMessageRegistry()
                );

                sut.Get("foo");
            }

            Assert.True(spy.WasDisposed);
        }
    }

    internal class ProducerConfigurationBuilder
    {
        private Func<KafkaProducer> _kafkaProducerFactory;

        public ProducerConfigurationBuilder WithKafkaProducerFactory(Func<KafkaProducer> kafkaProducerFactory)
        {
            _kafkaProducerFactory = kafkaProducerFactory;
            return this;
        }

        internal ProducerConfiguration Build()
        {
            var realBuilder = new Dafda.Configuration.ProducerConfigurationBuilder();
            realBuilder.WithBootstrapServers("dummy");
            
            if (_kafkaProducerFactory != null)
            {
                realBuilder.WithKafkaProducerFactory(_kafkaProducerFactory);
            }

            return realBuilder.Build();
        }
    }
}