using System;
using Dafda.Configuration;
using Dafda.Consuming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dafda.Tests.Consuming
{
    public class TestPoisonAwareIncomingMessageFactory
    {
        [Fact]
        public void Can_create_valid_transport_level_message()
        {
            var dummy = new object();
            var stub = new IncomingMessageFactoryStub(() => new TransportLevelMessage(new Metadata(), type => dummy));
            var sut = new PoisonAwareIncomingMessageFactory(NullLogger<PoisonAwareIncomingMessageFactory>.Instance, stub);

            var transportLevelMessage = sut.Create("");
            var data = transportLevelMessage.ReadDataAs(typeof(object));
            
            Assert.Same(dummy, data);
        }

        [Fact]
        public void Can_create_transport_level_poison_message()
        {
            var stub = new IncomingMessageFactoryStub(() => throw new Exception());
            var sut = new PoisonAwareIncomingMessageFactory(NullLogger<PoisonAwareIncomingMessageFactory>.Instance, stub);

            var transportLevelMessage = sut.Create("");

            var data = transportLevelMessage.ReadDataAs(typeof(object));

            Assert.IsType<TransportLevelPoisonMessage>(data);
        }

        [Fact]
        public void Can_register_poison_aware_inner_message_factory()
        {
            var configuration = new ConsumerConfigurationBuilder()
                .WithGroupId("foo")
                .WithBootstrapServers("bar")
                .WithPoisonMessageHandling()
                .Build();

            var provider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var incomingMessageFactory = configuration.IncomingMessageFactory(provider);

            Assert.IsType<PoisonAwareIncomingMessageFactory>(incomingMessageFactory);
        }

        private class IncomingMessageFactoryStub : IIncomingMessageFactory
        {
            private readonly Func<TransportLevelMessage> _factory;

            public IncomingMessageFactoryStub(Func<TransportLevelMessage> factory)
            {
                _factory = factory;
            }

            public TransportLevelMessage Create(string rawMessage)
            {
                return _factory();
            }
        }
    }
}