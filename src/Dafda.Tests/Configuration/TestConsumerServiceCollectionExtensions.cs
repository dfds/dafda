using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestConsumerServiceCollectionExtensions
    {
        [Fact( /*Skip = "is this relevant for testing these extensions"*/)]
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
            services.AddLogging();
            services.AddSingleton<IApplicationLifetime, DummyApplicationLifetime>();
            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId");
                options.RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage));

                options.WithConsumerScopeFactory(_ =>new ConsumerScopeFactoryStub(new ConsumerScopeStub(messageResult)));
            });
            var serviceProvider = services.BuildServiceProvider();

            var consumerHostedService = serviceProvider.GetServices<IHostedService>()
                .OfType<ConsumerHostedService>()
                .First();

            using (var cts = new CancellationTokenSource(50))
            {
                await consumerHostedService.ConsumeAll(cts.Token);
            }

            Assert.Equal(dummyMessage, DummyMessageHandler.LastHandledMessage);
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

        public class DummyMessage
        {
        }

        public class DummyMessageHandler : IMessageHandler<DummyMessage>
        {
            public Task Handle(DummyMessage message, MessageHandlerContext context)
            {
                LastHandledMessage = message;

                return Task.CompletedTask;
            }

            public static object LastHandledMessage { get; private set; }
        }
    }
}