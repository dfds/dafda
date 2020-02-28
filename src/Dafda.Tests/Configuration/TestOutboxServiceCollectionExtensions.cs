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

        public class DummyMessage
        {
        }
    }
}