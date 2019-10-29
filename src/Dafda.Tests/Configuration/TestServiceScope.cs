using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Messaging;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestServiceScope
    {
        [Fact]
        public async Task Has_expected_number_of_creations_and_disposals_when_transient()
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
            services.AddTransient<Repository>();
            services.AddSingleton<IApplicationLifetime, DummyApplicationLifetime>();
            services.AddTransient<DummyMessageHandler>();
            services.AddLogging();
            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId");
                options.RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage));

                options.WithTopicSubscriberScopeFactory(new TopicSubscriberScopeFactoryStub(new TopicSubscriberScopeStub(messageResult)));
            });

            services.AddTransient<ScopeSpy>();

            var serviceProvider = services.BuildServiceProvider();
            var consumer = serviceProvider.GetRequiredService<Consumer>();

            ScopeSpy.Reset();

            await consumer.ConsumeSingle(CancellationToken.None);
            await consumer.ConsumeSingle(CancellationToken.None);

            Assert.Equal(4, ScopeSpy.Created);
            Assert.Equal(4, ScopeSpy.Disposed);
        }

        [Fact]
        public async Task Has_expected_number_of_creations_and_disposals_when_singleton()
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
            services.AddTransient<Repository>();
            services.AddSingleton<IApplicationLifetime, DummyApplicationLifetime>();
            services.AddTransient<DummyMessageHandler>();
            services.AddLogging();
            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId");
                options.RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage));

                options.WithTopicSubscriberScopeFactory(new TopicSubscriberScopeFactoryStub(new TopicSubscriberScopeStub(messageResult)));
            });

            services.AddSingleton<ScopeSpy>();

            var serviceProvider = services.BuildServiceProvider();
            var consumer = serviceProvider.GetRequiredService<Consumer>();

            ScopeSpy.Reset();

            await consumer.ConsumeSingle(CancellationToken.None);
            await consumer.ConsumeSingle(CancellationToken.None);

            Assert.Equal(1, ScopeSpy.Created);
            Assert.Equal(0, ScopeSpy.Disposed);
        }

        [Fact]
        public async Task Has_expected_number_of_creations_and_disposals_when_scoped()
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
            services.AddTransient<Repository>();
            services.AddSingleton<IApplicationLifetime, DummyApplicationLifetime>();
            services.AddTransient<DummyMessageHandler>();
            services.AddLogging();
            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId");
                options.RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage));

                options.WithTopicSubscriberScopeFactory(new TopicSubscriberScopeFactoryStub(new TopicSubscriberScopeStub(messageResult)));
            });

            services.AddScoped<ScopeSpy>();

            var serviceProvider = services.BuildServiceProvider();
            var consumer = serviceProvider.GetRequiredService<Consumer>();

            ScopeSpy.Reset();

            await consumer.ConsumeSingle(CancellationToken.None);
            await consumer.ConsumeSingle(CancellationToken.None);

            Assert.Equal(2, ScopeSpy.Created);
            Assert.Equal(2, ScopeSpy.Disposed);
        }

        public class DummyMessage
        {
        }

        public class DummyMessageHandler : IMessageHandler<DummyMessage>
        {
            private readonly ScopeSpy _scopeSpy;
            private readonly Repository _repository;

            public DummyMessageHandler(ScopeSpy scopeSpy, Repository repository)
            {
                _scopeSpy = scopeSpy;
                _repository = repository;
            }

            public async Task Handle(DummyMessage message)
            {
                await _scopeSpy.DoSomethingAsync();
                await _repository.PerformActionAsync();
            }
        }

        public class Repository
        {
            private readonly ScopeSpy _scopeSpy;

            public Repository(ScopeSpy scopeSpy)
            {
                _scopeSpy = scopeSpy;
            }

            public async Task PerformActionAsync()
            {
                await _scopeSpy.DoSomethingAsync();
                await Task.Delay(500);
            }
        }

        public class ScopeSpy : IDisposable
        {
            private bool _diposed;

            public static int Created { get; private set; }
            public static int Disposed { get; private set; }

            public ScopeSpy()
            {
                Created++;
            }

            public void Dispose()
            {
                _diposed = true;
                Disposed++;
            }

            public static void Reset()
            {
                Created = Disposed = 0;
            }

            public async Task DoSomethingAsync()
            {
                if (_diposed)
                {
                    throw new ObjectDisposedException(nameof(ScopeSpy), "Ups, already disposed!");
                }

                await Task.Delay(500);
            }
        }
    }
}