﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
using Dafda.Consuming.Handlers;
using Dafda.Consuming.Interfaces;
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
            services.AddSingleton<IHostApplicationLifetime, DummyApplicationLifetime>();
            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId");
                options.RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage));

                options.WithConsumerScopeFactory(_ => new ConsumerScopeFactoryStub(new ConsumerScopeStub(messageResult)));
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
        public void add_single_consumer_registeres_a_single_hosted_service()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IHostApplicationLifetime, DummyApplicationLifetime>();
            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId");
            });

            var serviceProvider = services.BuildServiceProvider();
            var consumerHostedServices = serviceProvider
                .GetServices<IHostedService>()
                .OfType<ConsumerHostedService>();

            Assert.Single(consumerHostedServices);
        }

        [Fact]
        public void add_multiple_consumers_registeres_multiple_hosted_services()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddSingleton<IHostApplicationLifetime, DummyApplicationLifetime>();

            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId 1");
            });

            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId 2");
            });

            var serviceProvider = services.BuildServiceProvider();
            var consumerHostedServices = serviceProvider
                .GetServices<IHostedService>()
                .OfType<ConsumerHostedService>();

            Assert.Equal(2, consumerHostedServices.Count());
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
        public async Task default_consumer_failure_strategy_will_stop_application()
        {
            var spy = new ApplicationLifetimeSpy();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IHostApplicationLifetime>(_ => spy);
            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId");
                options.WithConsumerScopeFactory(_ => new FailingConsumerScopeFactory());
            });
            var serviceProvider = services.BuildServiceProvider();

            var consumerHostedService = serviceProvider.GetServices<IHostedService>()
                .OfType<ConsumerHostedService>()
                .Single();

            await consumerHostedService.ConsumeAll(CancellationToken.None);

            Assert.True(spy.StopApplicationWasCalled);
        }

        [Fact]
        public async Task consumer_failure_strategy_is_evaluated()
        {
            const int failuresBeforeQuitting = 2;
            var count = 0;

            var spy = new ApplicationLifetimeSpy();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IHostApplicationLifetime>(_ => spy);
            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId");
                options.WithConsumerScopeFactory(_ => new FailingConsumerScopeFactory());
                options.WithConsumerErrorHandler(exception =>
                {
                    if (++count > failuresBeforeQuitting)
                    {
                        return Task.FromResult(ConsumerFailureStrategy.Default);
                    }

                    return Task.FromResult(ConsumerFailureStrategy.RestartConsumer);
                });
            });
            var serviceProvider = services.BuildServiceProvider();

            var consumerHostedService = serviceProvider.GetServices<IHostedService>()
                .OfType<ConsumerHostedService>()
                .Single();

            await consumerHostedService.ConsumeAll(CancellationToken.None);

            Assert.Equal(failuresBeforeQuitting + 1, count);
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

        private class FailingConsumerScopeFactory : IConsumerScopeFactory<MessageResult>
        {
            public IConsumerScope<MessageResult> CreateConsumerScope()
            {
                throw new System.InvalidOperationException();
            }
        }
    }
}