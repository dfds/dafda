using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Producing;
using Dafda.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestServiceCollectionExtensions
    {
        [Fact]
        public void registers_a_producer_factory()
        {
            var services = new ServiceCollection();

            services.AddProducerFor<SimpleSender>(options =>
            {
                options.WithBootstrapServers("dummy");
            });

            var provider = services.BuildServiceProvider();
            var producerFactory = provider.GetService<ProducerFactory>();

            Assert.NotNull(producerFactory);
        }

        [Fact]
        public async Task Can_produce_message()
        {
            var spy = new KafkaProducerSpy();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddProducerFor<SimpleSender>(options =>
            {
                options.WithBootstrapServers("dummy");
                options.WithKafkaProducerFactory(_ => spy);
                options.Register<DummyMessage>("foo", "bar", x => "baz");
            });

            var provider = services.BuildServiceProvider();

            var simpleSender = provider.GetRequiredService<SimpleSender>();
            var producer = simpleSender.Producer;

            await producer.Produce(new DummyMessage());

            Assert.Equal("foo", spy.Topic);
            Assert.Equal("baz", spy.Key);
        }

        [Fact]
        public void registers_a_typed_producer()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddTransient<MessageSenderOne.AnotherDependency>();

            services.AddProducerFor<MessageSenderOne>(options =>
            {
                options.WithBootstrapServers("dummy");
            });

            var provider = services.BuildServiceProvider();
            var messageSender = provider.GetRequiredService<MessageSenderOne>();
            
            Assert.NotNull(messageSender);
            Assert.NotNull(messageSender.Producer);
            Assert.NotNull(messageSender.ADependency);
            
            Assert.Equal("hello one", messageSender.ADependency.Message);
            Assert.Equal(ProducerFactory.GetKeyNameOf<MessageSenderOne>(), messageSender.Producer.Name);
        }

        [Fact]
        public void resolves_a_typed_producer_for_an_abstract_service()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddTransient<MessageSenderOne.AnotherDependency>();
            services.AddProducerFor<IMessageSenderOne, MessageSenderOne>(options =>
            {
                options.WithBootstrapServers("dummy");
            });
            var provider = services.BuildServiceProvider();
            var messageSender = provider.GetRequiredService<IMessageSenderOne>();

            Assert.NotNull(messageSender);
            Assert.NotNull(messageSender.Producer);
            Assert.Equal(ProducerFactory.GetKeyNameOf<MessageSenderOne>(), messageSender.Producer.Name);
        }

        [Fact]
        public void registers_multiple_typed_producer()
        {
            var services = new ServiceCollection();
            
            services.AddLogging();
            services.AddTransient<MessageSenderOne.AnotherDependency>();
            services.AddProducerFor<MessageSenderOne>(options =>
            {
                options.WithBootstrapServers("dummy");
            });

            services.AddTransient<MessageSenderTwo.AnotherDependency>();
            services.AddProducerFor<MessageSenderTwo>(options =>
            {
                options.WithBootstrapServers("dummy");
            });

            var provider = services.BuildServiceProvider();
            
            var messageSenderOne = provider.GetRequiredService<MessageSenderOne>();
            
            Assert.NotNull(messageSenderOne);
            Assert.NotNull(messageSenderOne.Producer);
            Assert.NotNull(messageSenderOne.ADependency);
            
            Assert.Equal("hello one", messageSenderOne.ADependency.Message);
            Assert.Equal(ProducerFactory.GetKeyNameOf<MessageSenderOne>(), messageSenderOne.Producer.Name);

            var messageSenderTwo = provider.GetRequiredService<MessageSenderTwo>();
            
            Assert.NotNull(messageSenderTwo);
            Assert.NotNull(messageSenderTwo.Producer);
            Assert.NotNull(messageSenderTwo.ADependency);
            
            Assert.Equal("hello two", messageSenderTwo.ADependency.Message);
            Assert.Equal(ProducerFactory.GetKeyNameOf<MessageSenderTwo>(), messageSenderTwo.Producer.Name);
            
            Assert.NotEqual(messageSenderOne.Producer.Name, messageSenderTwo.Producer.Name);
        }

        [Fact]
        public void does_not_register_a_producer_directly()
        {
            var services = new ServiceCollection();
            
            services.AddProducerFor<SimpleSender>(options =>
            {
                options.WithBootstrapServers("dummy");
            });

            var provider = services.BuildServiceProvider();
            var producer = provider.GetService<Producer>();

            Assert.Null(producer);
        }

        #region helper classes

        private class SimpleSender
        {
            public SimpleSender(Producer producer)
            {
                Producer = producer;
            }
            
            public Producer Producer { get; private set; }
        }

        private interface IMessageSenderOne
        {
            public Producer Producer { get; }
        }

        private class MessageSenderOne : IMessageSenderOne
        {
            public MessageSenderOne(Producer producer, AnotherDependency anotherDependency)
            {
                ADependency = anotherDependency;
                Producer = producer;
            }
            
            public Producer Producer { get; }
            public AnotherDependency ADependency { get; }

            public class AnotherDependency
            {
                public string Message => "hello one";
            }
        }

        private class MessageSenderTwo
        {
            public MessageSenderTwo(Producer producer, AnotherDependency anotherDependency)
            {
                ADependency = anotherDependency;
                Producer = producer;
            }
            
            public Producer Producer { get; }
            public AnotherDependency ADependency { get; }

            public class AnotherDependency
            {
                public string Message => "hello two";
            }
        }

        #endregion

        public class DummyMessage
        {
        }
    }
}