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
                options.WithNotifier(serviceProvider => dummyNotification);
            });
            services.AddOutboxProducer(options =>
            {
                options.WithBootstrapServers("localhost");
                options.WithKafkaProducerFactory(() => spy);
                options.WithUnitOfWorkFactory(serviceProvider => fake);
                options.WithNotification(serviceProvider => dummyNotification);
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

        private class DummyNotification : IOutboxNotification
        {
            public void Notify()
            {
            }

            public void Wait()
            {
            }
        }
    }
}