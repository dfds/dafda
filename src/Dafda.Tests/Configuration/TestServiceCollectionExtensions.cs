using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
using Dafda.Outbox;
using Dafda.Producing;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestServiceCollectionExtensions
    {
        [Fact(Skip = "is this relevant for testing these extensions")]
        public async Task Can_consume_message()
        {
            var dummyMessage = new DummyMessage();
            var messageStub = new TransportLevelMessageBuilder()
                .WithType(nameof(DummyMessage))
                .WithData(dummyMessage)
                .Build();
            var messageResult = new MessageResultBuilder()
                .WithTransportLevelMessage(messageStub)
                .Build();

            var services = new ServiceCollection();
            services.AddScoped<Scoped>();
            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId");
                options.RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage));

                options.WithConsumerScopeFactory(new ConsumerScopeFactoryStub(new ConsumerScopeStub(messageResult)));
            });
            var serviceProvider = services.BuildServiceProvider();

            var consumer = serviceProvider.GetRequiredService<Consumer>();

            await consumer.ConsumeSingle(CancellationToken.None);

            Assert.Equal(dummyMessage, DummyMessageHandler.LastHandledMessage);
        }

        [Fact(Skip = "is this relevant for testing these extensions")]
        public async Task Can_consume_message_2()
        {
            var dummyMessage = new DummyMessage();
            var messageStub = new TransportLevelMessageBuilder()
                .WithType(nameof(DummyMessage))
                .WithData(dummyMessage)
                .Build();
            var messageResult = new MessageResultBuilder()
                .WithTransportLevelMessage(messageStub)
                .Build();

            var instances = 0;
            
            var services = new ServiceCollection();
            services.AddScoped<Scoped>(provider => new Scoped(() => ++instances));
            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId");
                options.RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage));
                options.WithConsumerScopeFactory(new ConsumerScopeFactoryStub(new ConsumerScopeStub(messageResult)));
            });
            var serviceProvider = services.BuildServiceProvider();

            var consumer = serviceProvider.GetRequiredService<Consumer>();

            await consumer.ConsumeSingle(CancellationToken.None);

            Assert.Equal(dummyMessage, DummyMessageHandler.LastHandledMessage);
            Assert.Equal(1, instances);
        }

        [Fact]
        public void throws_exception_when_registering_multiple_consumers_with_same_consumer_group_id()
        {
            var consumerGroupId = "foo";
            
            var services = new ServiceCollection();
            services.AddConsumer(options =>
            {
                options.WithGroupId(consumerGroupId);
                options.WithBootstrapServers("dummy");
            });

            Assert.Throws<InvalidConfigurationException>(() =>
            {
                services.AddConsumer(options =>
                {
                    options.WithBootstrapServers("dummy");
                    options.WithGroupId(consumerGroupId);
                });
            });
        }

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
            services.AddProducerFor<SimpleSender>(options =>
            {
                options.WithBootstrapServers("dummy");
                options.WithKafkaProducerFactory(() => spy);
                options.WithMessageIdGenerator(new MessageIdGeneratorStub(() => "qux"));
                options.Register<DummyMessage>("foo", "bar", x => "baz");
            });

            var provider = services.BuildServiceProvider();

            var simpleSender = provider.GetRequiredService<SimpleSender>();
            var producer = simpleSender.Producer;

            await producer.Produce(new DummyMessage());

            Assert.Equal("foo", spy.LastMessage.Topic);
            Assert.Equal("qux", spy.LastMessage.MessageId);
            Assert.Equal("bar", spy.LastMessage.Type);
            Assert.Equal("baz", spy.LastMessage.Key);
        }

        [Fact]
        public void registers_a_typed_producer()
        {
            var services = new ServiceCollection();
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
        public void registers_multiple_typed_producer()
        {
            var services = new ServiceCollection();
            
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

        [Fact]
        public async Task Can_produce_outbox_message()
        {
            var spy = new KafkaProducerSpy();
            var services = new ServiceCollection();
            var fake = new FakeOutboxPersistence();
            var messageId = Guid.NewGuid().ToString();
            services.AddLogging();
            services.AddOutbox(options =>
            {
                options.WithBootstrapServers("localhost");
                options.WithKafkaProducerFactory(() => spy);
                options.WithMessageIdGenerator(new MessageIdGeneratorStub(() => messageId));
                options.Register<DummyMessage>("foo", "bar", x => "baz");

                options.WithOutboxMessageRepository(serviceProvider => fake);
                options.WithUnitOfWorkFactory(serviceProvider => fake);
            });
            var provider = services.BuildServiceProvider();
            var outbox = provider.GetRequiredService<OutboxQueue>();

            await outbox.Enqueue(new[] {new DummyMessage(),});

            var pollingPublisher = provider
                .GetServices<IHostedService>()
                .Where(x => x is OutboxDispatcherHostedService)
                .Cast<OutboxDispatcherHostedService>()
                .First();

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(500);

                await pollingPublisher.ProcessUnpublishedOutboxMessages(cts.Token);
            }

            Assert.True(fake.OutboxMessages.All(x => x.ProcessedUtc.HasValue));

            Assert.Equal("foo", spy.LastMessage.Topic);
            Assert.Equal(messageId, spy.LastMessage.MessageId);
            Assert.Equal("bar", spy.LastMessage.Type);
            Assert.Equal("baz", spy.LastMessage.Key);
//            Assert.Equal("", spy.LastOutgoingMessage.Value);
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

        private class MessageSenderOne
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
    }

    public class FakeOutboxPersistence : IOutboxMessageRepository, IOutboxUnitOfWorkFactory
    {
        public List<OutboxMessage> OutboxMessages { get; }

        public FakeOutboxPersistence(params OutboxMessage[] outboxMessages)
        {
            OutboxMessages = outboxMessages.ToList();
        }

        public Task Add(IEnumerable<OutboxMessage> outboxMessages)
        {
            OutboxMessages.AddRange(outboxMessages);
            return Task.CompletedTask;
        }

        public IOutboxUnitOfWork Begin()
        {
            return new FakeUnitOfWork(this);
        }

        public bool Committed { get; private set; }

        private class FakeUnitOfWork : IOutboxUnitOfWork
        {
            private readonly FakeOutboxPersistence _fake;

            public FakeUnitOfWork(FakeOutboxPersistence fake)
            {
                _fake = fake;
            }

            public Task<ICollection<OutboxMessage>> GetAllUnpublishedMessages(CancellationToken stoppingToken)
            {
                return Task.FromResult<ICollection<OutboxMessage>>(_fake.OutboxMessages.Where(x => x.ProcessedUtc == null).ToList());
            }

            public Task Commit(CancellationToken stoppingToken)
            {
                _fake.Committed = true;
                return Task.CompletedTask;
            }

            public void Dispose()
            {
            }
        }
    }

    public class Scoped
    {
        public Scoped(Action onCreated =null)
        {
            onCreated?.Invoke();
        }
    }

    public class DummyMessage
    {
    }

    public class DummyMessageHandler : IMessageHandler<DummyMessage>
    {
        public DummyMessageHandler(Scoped scoped)
        {
        }

        public Task Handle(DummyMessage message, MessageHandlerContext context)
        {
            LastHandledMessage = message;

            return Task.CompletedTask;
        }

        public static object LastHandledMessage { get; private set; }
    }
}