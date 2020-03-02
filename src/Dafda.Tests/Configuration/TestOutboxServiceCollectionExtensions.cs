using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Outbox;
using Dafda.Producing;
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

            services.AddOutboxProducer(options =>
            {
                options.WithBootstrapServers("localhost");
                options.WithKafkaProducerFactory(() => new KafkaProducerSpy());
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
            var messageId = Guid.NewGuid().ToString();

            services.AddLogging();
            services.AddOutbox(options =>
            {
                options.WithMessageIdGenerator(new MessageIdGeneratorStub(() => messageId));
                options.Register<DummyMessage>("foo", "bar", x => "baz");

                options.WithOutboxMessageRepository(serviceProvider => fake);
                options.WithNotifier(dummyNotification);
            });
            services.AddOutboxProducer(options =>
            {
                options.WithBootstrapServers("localhost");
                options.WithKafkaProducerFactory(() => spy);
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

            Assert.Equal("foo", spy.LastMessage.Topic);
            Assert.Equal(messageId, spy.LastMessage.MessageId);
            Assert.Equal("bar", spy.LastMessage.Type);
            Assert.Equal("baz", spy.LastMessage.Key);
//            Assert.Equal("", spy.LastOutgoingMessage.Value);
        }

        public class DummyMessage
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