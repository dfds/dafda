using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Outbox;
using Dafda.Producing;
using Dafda.Serializing;
using Dafda.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestOutboxServiceCollectionExtensions
    {
        [Fact]
        public async Task Can_persist_outbox_message()
        {
            var services = new ServiceCollection();
            var fake = new FakeOutboxPersistence();
            var messageId = Guid.NewGuid();

            services.AddOutbox(options =>
            {
                options.WithMessageIdGenerator(new MessageIdGeneratorStub(() => messageId.ToString()));
                options.Register<DummyMessage>("foo", "bar", x => "baz");

                options.WithOutboxMessageRepository(serviceProvider => fake);
                options.WithNotifier(new DummyNotification());
            });
            var provider = services.BuildServiceProvider();
            var outbox = provider.GetRequiredService<OutboxQueue>();

            await outbox.Enqueue(new[] {new DummyMessage()});

            var outboxMessage = fake.OutboxMessages.Single();

            Assert.Equal("foo", outboxMessage.Topic);
            Assert.Equal("bar", outboxMessage.Type);
            Assert.Equal(messageId, outboxMessage.MessageId);
            Assert.Equal("baz", outboxMessage.Key);
            Assert.Equal("application/json", outboxMessage.Format);
            //Assert.Equal("null", outboxMessage.CorrelationId); // TODO -- should probably be testable
            Assert.NotNull(outboxMessage.Data); // TODO -- do we need to test message serialization here, or could it just be a canned answer for testability?
            //Assert.Equal(DateTime.Now, outboxMessage.OccurredOnUtc);  // TODO -- should probably be testable
            Assert.Null(outboxMessage.ProcessedUtc);
        }
        
        [Fact]
        public async Task Can_persist_outbox_message_with_default_serializer_format()
        {
            var services = new ServiceCollection();
            var fake = new FakeOutboxPersistence();

            services.AddOutbox(options =>
            {
                options.Register<DummyMessage>("foo", "bar", x => "baz");
                options.WithOutboxMessageRepository(serviceProvider => fake);
            });

            var provider = services.BuildServiceProvider();
            var outbox = provider.GetRequiredService<OutboxQueue>();

            await outbox.Enqueue(new[] {new DummyMessage()});

            var outboxMessage = fake.OutboxMessages.Single();

            Assert.Equal("application/json", outboxMessage.Format);
        }

        [Fact]
        public async Task Can_persist_outbox_message_with_special_serializer_format()
        {
            var services = new ServiceCollection();
            var fake = new FakeOutboxPersistence();

            services.AddOutbox(options =>
            {
                options.Register<DummyMessage>("foo", "bar", x => "baz");
                options.WithOutboxMessageRepository(serviceProvider => fake);
                options.WithPayloadSerializer("foo", new PayloadSerializerStub("dummy", "expected payload format"));
            });

            var provider = services.BuildServiceProvider();
            var outbox = provider.GetRequiredService<OutboxQueue>();

            await outbox.Enqueue(new[] {new DummyMessage()});

            var outboxMessage = fake.OutboxMessages.Single();

            Assert.Equal("expected payload format", outboxMessage.Format);
        }

        [Fact]
        public async Task Can_persist_outbox_messages_with_different_serializer_formats()
        {
            var services = new ServiceCollection();
            var fake = new FakeOutboxPersistence();

            services.AddOutbox(options =>
            {
                options.WithOutboxMessageRepository(serviceProvider => fake);
                
                options.Register<DummyMessage>("foo", "bar", x => "baz");
                options.WithPayloadSerializer("foo", new PayloadSerializerStub("dummy", "expected foo payload format"));

                options.Register<AnotherDummyMessage>("bar", "bar", x => "baz");
            });

            var provider = services.BuildServiceProvider();
            var outbox = provider.GetRequiredService<OutboxQueue>();

            await outbox.Enqueue(new[] {new DummyMessage()});
            await outbox.Enqueue(new[] {new AnotherDummyMessage()});

            Assert.Equal(
                expected: new[]
                {
                    "expected foo payload format",
                    new DefaultPayloadSerializer().PayloadFormat, 
                },
                actual: fake.OutboxMessages.Select(x => x.Format)
            );
        }

        [Fact]
        public async Task Can_configure_outbox_notifier()
        {
            var services = new ServiceCollection();
            var dummyOutboxNotifier = new DummyNotification();

            services.AddOutbox(options =>
            {
                options.WithNotifier(dummyOutboxNotifier);
                options.Register<DummyMessage>("foo", "bar", x => "baz");

                options.WithOutboxMessageRepository(serviceProvider => new FakeOutboxPersistence());
            });
            var provider = services.BuildServiceProvider();
            var outbox = provider.GetRequiredService<OutboxQueue>();

            var outboxNotifier = await outbox.Enqueue(new[] {new DummyMessage()});

            Assert.Same(dummyOutboxNotifier, outboxNotifier);
        }

        [Fact]
        public void Producer_can_wait_for_notification()
        {
            var services = new ServiceCollection();
            var spy = new OutboxListenerSpy();

            services.AddLogging();
            services.AddOutboxProducer(options =>
            {
                options.WithBootstrapServers("localhost");
                options.WithKafkaProducerFactory(_ => new KafkaProducerSpy());
                options.WithListener(spy);
                options.WithUnitOfWorkFactory(serviceProvider => new FakeOutboxPersistence());
            });
            var provider = services.BuildServiceProvider();

            var pollingPublisher = provider
                .GetServices<IHostedService>()
                .OfType<OutboxDispatcherHostedService>()
                .First();

            using (var cts = new CancellationTokenSource(10))
            {
                pollingPublisher.ProcessOutbox(cts.Token);
            }

            Assert.True(spy.Waited);
        }

        [Fact]
        public async Task Can_produce_outbox_message()
        {
            var spy = new KafkaProducerSpy();
            var services = new ServiceCollection();
            var fake = new FakeOutboxPersistence();
            var dummyNotification = new DummyNotification();

            services.AddLogging();
            services.AddOutbox(options =>
            {

                options.Register<DummyMessage>("foo", "bar", x => "baz");

                options.WithOutboxMessageRepository(serviceProvider => fake);
                options.WithNotifier(dummyNotification);
            });
            services.AddOutboxProducer(options =>
            {
                options.WithBootstrapServers("localhost");
                options.WithKafkaProducerFactory(_ => spy);
                options.WithUnitOfWorkFactory(serviceProvider => fake);
                options.WithListener(dummyNotification);
            });

            var provider = services.BuildServiceProvider();
            var outbox = provider.GetRequiredService<OutboxQueue>();

            await outbox.Enqueue(new[] {new DummyMessage()});

            var pollingPublisher = provider
                .GetServices<IHostedService>()
                .OfType<OutboxDispatcherHostedService>()
                .First();

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(10);

                pollingPublisher.ProcessOutbox(cts.Token);
            }

            Assert.True(fake.OutboxMessages.All(x => x.ProcessedUtc.HasValue));

            Assert.True(fake.Committed);

            Assert.Equal("foo", spy.Topic);
            Assert.Equal("baz", spy.Key);
        }

        public class DummyMessage
        {
        }

        public class AnotherDummyMessage
        {
        }

        private class DummyNotification : IOutboxListener, IOutboxNotifier
        {
            public void Notify()
            {
            }

            public bool Wait(CancellationToken cancellationToken)
            {
                return false;
            }
        }

        public class OutboxListenerSpy : IOutboxListener
        {
            public bool Wait(CancellationToken cancellationToken)
            {
                Waited = true;
                return true;
            }

            public bool Waited { get; private set; }
        }
    }
}