using System;
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
            services.AddSingleton<IHostApplicationLifetime, DummyApplicationLifetime>();
            services.AddTransient<DummyMessageHandler>();
            services.AddLogging();

            var createCount = 0;
            var disposeCount = 0;

            services.AddTransient<ScopeSpy>(provider => new ScopeSpy(onCreate: () => createCount++, onDispose: () => disposeCount++));

            var serviceProvider = services.BuildServiceProvider();

            var consumerConfiguration = new ConsumerConfigurationBuilder()
                .WithGroupId("dummy")
                .WithBootstrapServers("dummy")
                .RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage))
                .WithUnitOfWorkFactory(new ServiceProviderUnitOfWorkFactory(serviceProvider))
                .Build();

            var consumer = new ConsumerBuilder()
                .WithMessageHandlerRegistry(consumerConfiguration.MessageHandlerRegistry)
                .WithUnitOfWorkFactory(consumerConfiguration.UnitOfWorkFactory)
                .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(new ConsumerScopeStub(messageResult)))
                .WithEnableAutoCommit(consumerConfiguration.EnableAutoCommit)
                .Build();

            await consumer.ConsumeSingle(CancellationToken.None);
            await consumer.ConsumeSingle(CancellationToken.None);

            Assert.Equal(4, createCount);
            Assert.Equal(4, disposeCount);
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
            services.AddSingleton<IHostApplicationLifetime, DummyApplicationLifetime>();
            services.AddTransient<DummyMessageHandler>();
            services.AddLogging();

            var createCount = 0;
            var disposeCount = 0;

            services.AddSingleton<ScopeSpy>(provider => new ScopeSpy(onCreate: () => createCount++, onDispose: () => disposeCount++));

            var serviceProvider = services.BuildServiceProvider();

            var consumerConfiguration = new ConsumerConfigurationBuilder()
                .WithGroupId("dummy")
                .WithBootstrapServers("dummy")
                .RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage))
                .WithUnitOfWorkFactory(new ServiceProviderUnitOfWorkFactory(serviceProvider))
                .Build();

            var consumer = new ConsumerBuilder()
                .WithMessageHandlerRegistry(consumerConfiguration.MessageHandlerRegistry)
                .WithUnitOfWorkFactory(consumerConfiguration.UnitOfWorkFactory)
                .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(new ConsumerScopeStub(messageResult)))
                .WithEnableAutoCommit(consumerConfiguration.EnableAutoCommit)
                .Build();

            await consumer.ConsumeSingle(CancellationToken.None);
            await consumer.ConsumeSingle(CancellationToken.None);

            Assert.Equal(1, createCount);
            Assert.Equal(0, disposeCount);
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
            services.AddSingleton<IHostApplicationLifetime, DummyApplicationLifetime>();
            services.AddTransient<DummyMessageHandler>();
            services.AddLogging();

            var createCount = 0;
            var disposeCount = 0;

            services.AddScoped<ScopeSpy>(provider => new ScopeSpy(onCreate: () => createCount++, onDispose: () => disposeCount++));

            var serviceProvider = services.BuildServiceProvider();

            var consumerConfiguration = new ConsumerConfigurationBuilder()
                .WithGroupId("dummy")
                .WithBootstrapServers("dummy")
                .RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage))
                .WithUnitOfWorkFactory(new ServiceProviderUnitOfWorkFactory(serviceProvider))
                .Build();

            var consumer = new ConsumerBuilder()
                .WithMessageHandlerRegistry(consumerConfiguration.MessageHandlerRegistry)
                .WithUnitOfWorkFactory(consumerConfiguration.UnitOfWorkFactory)
                .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(new ConsumerScopeStub(messageResult)))
                .WithEnableAutoCommit(consumerConfiguration.EnableAutoCommit)
                .Build();

            await consumer.ConsumeSingle(CancellationToken.None);
            await consumer.ConsumeSingle(CancellationToken.None);

            Assert.Equal(2, createCount);
            Assert.Equal(2, disposeCount);
        }

        [Fact]
        public async Task Has_expected_number_of_creations_and_disposals_when_scoped_2()
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
            services.AddSingleton<IHostApplicationLifetime, DummyApplicationLifetime>();
            services.AddTransient<DummyMessageHandler>();
            services.AddLogging();

            var createCount = 0;
            var disposeCount = 0;

            services.AddScoped<ScopeSpy>(provider => new ScopeSpy(onCreate: () => createCount++, onDispose: () => disposeCount++));

            var serviceProvider = services.BuildServiceProvider();

            var consumerConfiguration = new ConsumerConfigurationBuilder()
                .WithGroupId("dummy")
                .WithBootstrapServers("dummy")
                .RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage))
                .WithUnitOfWorkFactory(new ServiceProviderUnitOfWorkFactory(serviceProvider))
                .Build();

            var consumer = new ConsumerBuilder()
                .WithMessageHandlerRegistry(consumerConfiguration.MessageHandlerRegistry)
                .WithUnitOfWorkFactory(consumerConfiguration.UnitOfWorkFactory)
                .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(new ConsumerScopeStub(messageResult)))
                .WithEnableAutoCommit(consumerConfiguration.EnableAutoCommit)
                .Build();

            await consumer.ConsumeSingle(CancellationToken.None);
            await consumer.ConsumeSingle(CancellationToken.None);

            Assert.Equal(2, createCount);
            Assert.Equal(2, disposeCount);
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

            public async Task Handle(DummyMessage message, MessageHandlerContext context)
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
            }
        }

        public class ScopeSpy : IDisposable
        {
            private readonly Action _onCreate;
            private readonly Action _onDispose;

            private bool _diposed;

            public ScopeSpy(Action onCreate, Action onDispose)
            {
                _onCreate = onCreate;
                _onDispose = onDispose;

                _onCreate?.Invoke();
            }

            public void Dispose()
            {
                _diposed = true;
                _onDispose?.Invoke();
            }

            public async Task DoSomethingAsync()
            {
                if (_diposed)
                {
                    throw new ObjectDisposedException(nameof(ScopeSpy), "Ups, already disposed!");
                }

                await Task.Delay(10);
            }
        }
    }
}