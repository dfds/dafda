using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
using Dafda.Diagnostics;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
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

        [Fact( /*Skip = "is this relevant for testing these extensions"*/)]
        public async Task Can_consume_message_with_activity()
        {
            ConsumerActivitySource.Propagator = new CompositeTextMapPropagator(
                new TextMapPropagator[]
                {
                    new TraceContextPropagator(),
                    new BaggagePropagator()
                });

            var wasStarted = false;
            var wasStopped = false;

            var activityListener = new ActivityListener
            {
                ShouldListenTo = s => s.Name == "Dafda",
                SampleUsingParentId = (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllDataAndRecorded,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStarted = _ => { wasStarted = true; },
                ActivityStopped = _ => { wasStopped = true; }
            };
            ActivitySource.AddActivityListener(activityListener);

            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;

            var traceId = ActivityTraceId.CreateRandom();
            var spanId = ActivitySpanId.CreateRandom();
            var flags = (byte)ActivityTraceFlags.Recorded;
            var flagsChars = flags.ToString("x2");
            string parentId = "00-" + traceId + "-" + spanId + "-" + flagsChars;

            var dummyMessage = new DummyMessage();
            var messageStub = new TransportLevelMessageBuilder()
                .WithType(nameof(DummyMessage), parentId, "som=der")
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

            Assert.Equal(parentId, DummyMessageHandler.LastActivityParentId);
            Assert.Equal(new Dictionary<string, string>
            {
                { "som", "der" }
            }, DummyMessageHandler.LastBaggage.GetBaggage());

            Assert.True(wasStarted);
            Assert.True(wasStopped);
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
                LastActivityParentId = Activity.Current?.ParentId;
                LastActivityId = Activity.Current?.Id;
                LastBaggage = Baggage.Current;

                return Task.CompletedTask;
            }

            public static string? LastActivityParentId { get; private set; }
            public static string? LastActivityId { get; private set; }
            public static object LastHandledMessage { get; private set; }
            public static Baggage LastBaggage { get; private set; }
        }

        private class FailingConsumerScopeFactory : IConsumerScopeFactory
        {
            public ConsumerScope CreateConsumerScope()
            {
                throw new System.InvalidOperationException();
            }
        }
    }
}