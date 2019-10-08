using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Messaging;
using Dafda.Producing;
using Dafda.Tests.Builders;
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
            var configuration = serviceProvider.GetRequiredService<IConsumerConfiguration>();

            Assert.Equal("foo", configuration.FirstOrDefault(x => x.Key == "group.id").Value);
            Assert.Equal("bar", configuration.FirstOrDefault(x => x.Key == "bootstrap.servers").Value);
        }

        [Fact]
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

                options.WithTopicSubscriberScopeFactory(new TopicSubscriberScopeFactoryStub(new TopicSubscriberScopeStub(messageResult)));
            });
            var serviceProvider = services.BuildServiceProvider();

            var consumer = serviceProvider.GetRequiredService<Consumer>();

            await consumer.ConsumeSingle(CancellationToken.None);

            Assert.Equal(dummyMessage, DummyMessageHandler.LastHandledMessage);
        }

        [Fact]
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

                options.WithTopicSubscriberScopeFactory(new TopicSubscriberScopeFactoryStub(new TopicSubscriberScopeStub(messageResult)));
            });
            var serviceProvider = services.BuildServiceProvider();

            var consumer = serviceProvider.GetRequiredService<Consumer>();

            await consumer.ConsumeSingle(CancellationToken.None);

            Assert.Equal(dummyMessage, DummyMessageHandler.LastHandledMessage);
            Assert.Equal(1, Scoped.Instanciated);
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