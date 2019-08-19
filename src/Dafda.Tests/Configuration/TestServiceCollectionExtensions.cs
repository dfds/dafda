using System.Linq;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Messaging;
using Dafda.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestServiceCollectionExtensions
    {
        [Fact]
        public void Can_get_configuration()
        {
            var services = new ServiceCollection();
            services.AddConsumer(options =>
            {
                options.WithConfigurationSource(new ConfigurationStub(
                    ("SERVICE_GROUP_ID", "foo"),
                    ("DEFAULT_KAFKA_BOOTSTRAP_SERVERS", "bar"))
                );
                options.WithEnvironmentStyle("SERVICE", "DEFAULT_KAFKA");
            });

            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            Assert.Equal("foo", configuration.FirstOrDefault(x => x.Key == "group.id").Value);
            Assert.Equal("bar", configuration.FirstOrDefault(x => x.Key == "bootstrap.servers").Value);
        }

        [Fact]
        public async Task Can_dispatch_message_locally()
        {
            var services = new ServiceCollection();
            services
                .AddConsumer(options =>
                {
                    options.WithGroupId("foo");
                    options.WithBootstrapServers("localhost");
                    options.RegisterMessageHandler<DummyMessage, DummyMessageHandler>("foo", "bar");
                });

            services.AddSingleton<DummyMessageHandler>(); // NOTE: overwrite as singleton only for testing purposes
            services.AddTransient<IHandlerUnitOfWorkFactory, DefaultUnitOfWorkFactory>();

            var provider = services.BuildServiceProvider();
            var dispatcher = provider.GetRequiredService<ILocalMessageDispatcher>();

            var dummyMessage = new DummyMessage();
            await dispatcher.Dispatch(new TransportLevelMessageStub(dummyMessage, "bar"));

            var spy = provider.GetRequiredService<DummyMessageHandler>();

            Assert.Equal(dummyMessage, spy.LastHandledMessage);
        }

        public class DummyMessage
        {
        }

        public class DummyMessageHandler : IMessageHandler<DummyMessage>
        {
            public Task Handle(DummyMessage message)
            {
                LastHandledMessage = message;

                return Task.CompletedTask;
            }

            public object LastHandledMessage { get; private set; }
        }
    }
}