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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

            var services = new ServiceCollection();
            services.AddScoped<Scoped>();
            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId");
                options.RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage));

                options.WithUnitOfWork<UnitOfWork>();

                options.WithConsumerScopeFactory(new ConsumerScopeFactoryStub(new ConsumerScopeStub(messageResult)));
            });
            var serviceProvider = services.BuildServiceProvider();

            var consumer = serviceProvider.GetRequiredService<Consumer>();

            await consumer.ConsumeSingle(CancellationToken.None);

            Assert.Equal(dummyMessage, DummyMessageHandler.LastHandledMessage);
            Assert.Equal(1, Scoped.Instanciated);
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
        public async Task Can_produce_message()
        {
            var spy = new KafkaProducerSpy();
            var services = new ServiceCollection();

            services.AddProducer(options =>
            {
                options.WithBootstrapServers("localhost");
                options.WithKafkaProducerFactory(new KafkaProducerFactoryStub(spy));
                options.WithMessageIdGenerator(new MessageIdGeneratorStub(() => "qux"));
                options.Register<DummyMessage>("foo", "bar", x => "baz");
            });
            var provider = services.BuildServiceProvider();
            var producer = provider.GetRequiredService<IProducer>();

            await producer.Produce(new DummyMessage());

            Assert.Equal("foo", spy.LastMessage.Topic);
            Assert.Equal("qux", spy.LastMessage.MessageId);
            Assert.Equal("bar", spy.LastMessage.Type);
            Assert.Equal("baz", spy.LastMessage.Key);
//            Assert.Equal("", spy.LastOutgoingMessage.Value);
        }

        [Fact]
        public async Task Can_produce_outbox_message()
        {
            var spy = new KafkaProducerSpy();
            var services = new ServiceCollection();
            var fake = new FakeOutboxPersistence();
            var messageId = Guid.NewGuid().ToString();
            services.AddLogging();
            services.AddProducer(options =>
            {
                options.WithBootstrapServers("localhost");
                options.WithKafkaProducerFactory(new KafkaProducerFactoryStub(spy));
                options.WithMessageIdGenerator(new MessageIdGeneratorStub(() => messageId));
                options.Register<DummyMessage>("foo", "bar", x => "baz");

                options.AddOutbox(opt =>
                {
                    opt.WithOutboxMessageRepository(serviceProvider => fake);
                    opt.WithOutboxPublisher(pub => { pub.WithUnitOfWorkFactory(serviceProvider => fake); });
                });
            });
            var provider = services.BuildServiceProvider();
            var outbox = provider.GetRequiredService<IOutbox>();

            await outbox.Enqueue(new[] {new DummyMessage(),});

            var pollingPublisher = provider.GetServices<IHostedService>().FirstOrDefault(x => x is PollingPublisher);

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.CancelAfter(500);

                await pollingPublisher.StartAsync(cts.Token);
            }

            Assert.True(fake.OutboxMessages.All(x => x.ProcessedUtc.HasValue));

            Assert.Equal("foo", spy.LastMessage.Topic);
            Assert.Equal(messageId, spy.LastMessage.MessageId);
            Assert.Equal("bar", spy.LastMessage.Type);
            Assert.Equal("baz", spy.LastMessage.Key);
//            Assert.Equal("", spy.LastOutgoingMessage.Value);
        }
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

    public class UnitOfWork : ScopedUnitOfWork
    {
        public override Task ExecuteInScope(IServiceScope scope, Func<Task> execute)
        {
            var scoped = scope.ServiceProvider.GetRequiredService<Scoped>();

            return base.ExecuteInScope(scope, execute);
        }
    }

    public class Scoped
    {
        public static int Instanciated { get; set; }

        public Scoped()
        {
            Instanciated++;
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

        public Task Handle(DummyMessage message)
        {
            LastHandledMessage = message;

            return Task.CompletedTask;
        }

        public static object LastHandledMessage { get; private set; }
    }
}