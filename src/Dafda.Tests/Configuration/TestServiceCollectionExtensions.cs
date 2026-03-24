using System;
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
            Assert.Equal(typeof(MessageSenderOne).FullName, messageSender.Producer.Name);
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
            Assert.Equal(typeof(IMessageSenderOne).FullName, messageSender.Producer.Name);
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
            Assert.Equal(typeof(MessageSenderOne).FullName, messageSenderOne.Producer.Name);

            var messageSenderTwo = provider.GetRequiredService<MessageSenderTwo>();
            
            Assert.NotNull(messageSenderTwo);
            Assert.NotNull(messageSenderTwo.Producer);
            Assert.NotNull(messageSenderTwo.ADependency);
            
            Assert.Equal("hello two", messageSenderTwo.ADependency.Message);
            Assert.Equal(typeof(MessageSenderTwo).FullName, messageSenderTwo.Producer.Name);
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

        [Fact]
        public async Task Can_produce_message_with_service_provider_options_factory()
        {
            var spy = new KafkaProducerSpy();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddProducerFor<SimpleSender>(_ =>
            {
                var options = new ProducerOptions();
                options.WithBootstrapServers("dummy");
                options.WithKafkaProducerFactory(__ => spy);
                options.Register<DummyMessage>("foo", "bar", x => "baz");
                return options;
            });

            var provider = services.BuildServiceProvider();

            var simpleSender = provider.GetRequiredService<SimpleSender>();
            var producer = simpleSender.Producer;

            await producer.Produce(new DummyMessage());

            Assert.Equal("foo", spy.Topic);
            Assert.Equal("baz", spy.Key);
        }

        [Fact]
        public void registers_a_typed_producer_with_service_provider_options_factory()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddTransient<MessageSenderOne.AnotherDependency>();

            services.AddProducerFor<MessageSenderOne>(_ =>
            {
                var options = new ProducerOptions();
                options.WithBootstrapServers("dummy");
                return options;
            });

            var provider = services.BuildServiceProvider();
            var messageSender = provider.GetRequiredService<MessageSenderOne>();

            Assert.NotNull(messageSender);
            Assert.NotNull(messageSender.Producer);
            Assert.NotNull(messageSender.ADependency);

            Assert.Equal("hello one", messageSender.ADependency.Message);
            Assert.Equal(typeof(MessageSenderOne).FullName, messageSender.Producer.Name);
        }

        [Fact]
        public void resolves_a_typed_producer_for_an_abstract_service_with_service_provider_options_factory()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddTransient<MessageSenderOne.AnotherDependency>();
            services.AddProducerFor<IMessageSenderOne, MessageSenderOne>(_ =>
            {
                var options = new ProducerOptions();
                options.WithBootstrapServers("dummy");
                return options;
            });
            var provider = services.BuildServiceProvider();
            var messageSender = provider.GetRequiredService<IMessageSenderOne>();

            Assert.NotNull(messageSender);
            Assert.NotNull(messageSender.Producer);
            Assert.Equal(typeof(IMessageSenderOne).FullName, messageSender.Producer.Name);
        }

        [Fact]
        public void registers_multiple_typed_producers_with_service_provider_options_factory()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddTransient<MessageSenderOne.AnotherDependency>();
            services.AddProducerFor<MessageSenderOne>(_ =>
            {
                var options = new ProducerOptions();
                options.WithBootstrapServers("dummy");
                return options;
            });

            services.AddTransient<MessageSenderTwo.AnotherDependency>();
            services.AddProducerFor<MessageSenderTwo>(_ =>
            {
                var options = new ProducerOptions();
                options.WithBootstrapServers("dummy");
                return options;
            });

            var provider = services.BuildServiceProvider();

            var messageSenderOne = provider.GetRequiredService<MessageSenderOne>();

            Assert.NotNull(messageSenderOne);
            Assert.NotNull(messageSenderOne.Producer);
            Assert.NotNull(messageSenderOne.ADependency);

            Assert.Equal("hello one", messageSenderOne.ADependency.Message);
            Assert.Equal(typeof(MessageSenderOne).FullName, messageSenderOne.Producer.Name);

            var messageSenderTwo = provider.GetRequiredService<MessageSenderTwo>();

            Assert.NotNull(messageSenderTwo);
            Assert.NotNull(messageSenderTwo.Producer);
            Assert.NotNull(messageSenderTwo.ADependency);

            Assert.Equal("hello two", messageSenderTwo.ADependency.Message);
            Assert.Equal(typeof(MessageSenderTwo).FullName, messageSenderTwo.Producer.Name);
        }

        [Fact]
        public void does_not_register_a_producer_directly_with_service_provider_options_factory()
        {
            var services = new ServiceCollection();

            services.AddProducerFor<SimpleSender>(_ =>
            {
                var options = new ProducerOptions();
                options.WithBootstrapServers("dummy");
                return options;
            });

            var provider = services.BuildServiceProvider();
            var producer = provider.GetService<Producer>();

            Assert.Null(producer);
        }

        [Fact]
        public void producer_factory_is_singleton_reusing_same_kafka_producer_instance()
        {
            var spy = new KafkaProducerSpy();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddProducerFor<SimpleSender>(options =>
            {
                options.WithBootstrapServers("dummy");
                options.WithKafkaProducerFactory(_ => spy);
            });

            var provider = services.BuildServiceProvider();

            var senderOne = provider.GetRequiredService<SimpleSender>();
            var senderTwo = provider.GetRequiredService<SimpleSender>();

            Assert.Same(senderOne.Producer.KafkaProducer, senderTwo.Producer.KafkaProducer);
        }

        [Fact]
        public void producer_factory_is_singleton_reusing_same_kafka_producer_instance_with_service_provider_options_factory()
        {
            var spy = new KafkaProducerSpy();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddProducerFor<SimpleSender>(_ =>
            {
                var options = new ProducerOptions();
                options.WithBootstrapServers("dummy");
                options.WithKafkaProducerFactory(__ => spy);
                return options;
            });

            var provider = services.BuildServiceProvider();

            var senderOne = provider.GetRequiredService<SimpleSender>();
            var senderTwo = provider.GetRequiredService<SimpleSender>();

            Assert.Same(senderOne.Producer.KafkaProducer, senderTwo.Producer.KafkaProducer);
        }

        [Fact]
        public void disposing_service_provider_disposes_kafka_producer()
        {
            var spy = new KafkaProducerSpy();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddProducerFor<SimpleSender>(options =>
            {
                options.WithBootstrapServers("dummy");
                options.WithKafkaProducerFactory(_ => spy);
            });

            var provider = services.BuildServiceProvider();

            // Resolve the service so that the KafkaProducer is created inside ProducerFactory
            _ = provider.GetRequiredService<SimpleSender>();

            Assert.False(spy.WasDisposed);

            provider.Dispose();

            Assert.True(spy.WasDisposed);
        }

        [Fact]
        public void disposing_service_provider_does_not_throw_when_kafka_producer_was_never_created()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddProducerFor<SimpleSender>(options =>
            {
                options.WithBootstrapServers("dummy");
            });

            var provider = services.BuildServiceProvider();

            // Do NOT resolve SimpleSender — KafkaProducer is never instantiated
            var exception = Record.Exception(() => provider.Dispose());

            Assert.Null(exception);
        }

        [Fact]
        public void throws_when_same_service_type_is_registered_twice_with_action_options()
        {
            var services = new ServiceCollection();
            services.AddProducerFor<SimpleSender>(options =>
            {
                options.WithBootstrapServers("dummy");
            });

            var exception = Assert.Throws<ProducerFactoryException>(() =>
                services.AddProducerFor<SimpleSender>(options =>
                {
                    options.WithBootstrapServers("dummy");
                }));

            Assert.Contains(typeof(SimpleSender).FullName, exception.Message);
        }

        [Fact]
        public void throws_when_same_service_type_is_registered_twice_with_factory_options()
        {
            var services = new ServiceCollection();
            services.AddProducerFor<SimpleSender>(_ =>
            {
                var options = new ProducerOptions();
                options.WithBootstrapServers("dummy");
                return options;
            });

            var exception = Assert.Throws<ProducerFactoryException>(() =>
                services.AddProducerFor<SimpleSender>(_ =>
                {
                    var options = new ProducerOptions();
                    options.WithBootstrapServers("dummy");
                    return options;
                }));

            Assert.Contains(typeof(SimpleSender).FullName, exception.Message);
        }

        [Fact]
        public void throws_when_same_abstract_service_type_is_registered_twice_with_action_options()
        {
            var services = new ServiceCollection();
            services.AddTransient<MessageSenderOne.AnotherDependency>();
            services.AddProducerFor<IMessageSenderOne, MessageSenderOne>(options =>
            {
                options.WithBootstrapServers("dummy");
            });

            var exception = Assert.Throws<ProducerFactoryException>(() =>
                services.AddProducerFor<IMessageSenderOne, MessageSenderOne>(options =>
                {
                    options.WithBootstrapServers("dummy");
                }));

            Assert.Contains(typeof(IMessageSenderOne).FullName, exception.Message);
        }

        [Fact]
        public void throws_when_same_abstract_service_type_is_registered_twice_with_factory_options()
        {
            var services = new ServiceCollection();
            services.AddTransient<MessageSenderOne.AnotherDependency>();
            services.AddProducerFor<IMessageSenderOne, MessageSenderOne>(_ =>
            {
                var options = new ProducerOptions();
                options.WithBootstrapServers("dummy");
                return options;
            });

            var exception = Assert.Throws<ProducerFactoryException>(() =>
                services.AddProducerFor<IMessageSenderOne, MessageSenderOne>(_ =>
                {
                    var options = new ProducerOptions();
                    options.WithBootstrapServers("dummy");
                    return options;
                }));

            Assert.Contains(typeof(IMessageSenderOne).FullName, exception.Message);
        }

        [Fact]
        public void options_factory_receives_service_provider()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(new BootstrapServerSettings("dummy"));

            IServiceProvider capturedProvider = null;

            services.AddProducerFor<SimpleSender>(sp =>
            {
                capturedProvider = sp;
                var settings = sp.GetRequiredService<BootstrapServerSettings>();
                var options = new ProducerOptions();
                options.WithBootstrapServers(settings.BootstrapServers);
                return options;
            });

            var provider = services.BuildServiceProvider();
            _ = provider.GetRequiredService<SimpleSender>();

            Assert.NotNull(capturedProvider);
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

        private class BootstrapServerSettings
        {
            public BootstrapServerSettings(string bootstrapServers)
            {
                BootstrapServers = bootstrapServers;
            }

            public string BootstrapServers { get; }
        }

        #endregion

        public class DummyMessage
        {
        }
    }
}