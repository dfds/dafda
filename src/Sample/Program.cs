using System;
using System.Threading;
using Dafda.Configuration;
using Dafda.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.Application;
using Sample.Infrastructure.Persistence;
using Serilog;
using Serilog.Events;

namespace Sample
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Dafda", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting Sample application");

                CreateHostBuilder(args)
                    .Build()
                    .Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Sample application terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return new HostBuilder()
                .UseSerilog()
                .ConfigureHostConfiguration(config => { config.AddEnvironmentVariables(); })
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;

                    // configure main application
                    services.AddHostedService<MainWorker>();

                    services.AddSingleton<Stats>();

                    // configure persistence (PostgreSQL)
                    services.ConfigurePersistence(configuration["SAMPLE_DATABASE_CONNECTION_STRING"]);

                    // configure messaging: consumer
                    services.AddConsumer(options =>
                    {
                        // configuration settings
                        options.WithConfigurationSource(configuration);
                        options.WithEnvironmentStyle("DEFAULT_KAFKA");
                        options.WithEnvironmentStyle("SAMPLE_KAFKA");
                        options.WithGroupId("foo");

                        // register message handlers
                        options.RegisterMessageHandler<TestEvent, TestHandler>("test-topic", "test-event");
                    });

                    // configure ANOTHER messaging: consumer
                    services.AddConsumer(options =>
                    {
                        // configuration settings
                        options.WithConfigurationSource(configuration);
                        options.WithEnvironmentStyle("DEFAULT_KAFKA");
                        options.WithEnvironmentStyle("SAMPLE_KAFKA");
                        options.WithGroupId("bar");

                        // register message handlers
                        options.RegisterMessageHandler<TestEvent, AnotherTestHandler>("test-topic", "test-event");
                    });

                    var outboxNotification = new OutboxNotification(TimeSpan.FromSeconds(5));

                    services.AddSingleton(provider => outboxNotification); // register to dispose

                    // configure messaging: producer
                    services.AddOutbox(options =>
                    {
                        // register outgoing messages (includes outbox messages)
                        options.Register<TestEvent>("test-topic", "test-event", @event => @event.AggregateId);

                        // include outbox persistence
                        options.WithOutboxMessageRepository<OutboxMessageRepository>();
                        options.WithNotifier(outboxNotification);
                    });

                    services.AddOutboxProducer(options =>
                    {
                        // configuration settings
                        options.WithConfigurationSource(configuration);
                        options.WithEnvironmentStyle("DEFAULT_KAFKA");
                        options.WithEnvironmentStyle("SAMPLE_KAFKA");

                        // include outbox (polling publisher)
                        options.WithUnitOfWorkFactory<OutboxUnitOfWorkFactory>();
                        options.WithListener(outboxNotification);
                    });
                });
        }
    }

    public class Stats
    {
        private long _produced;
        private long _consumed;

        public long Produced => Interlocked.Read(ref _produced);
        public long Consumed => Interlocked.Read(ref _consumed);

        public void Produce()
        {
            Interlocked.Increment(ref _produced);
        }

        public void Consume()
        {
            Interlocked.Increment(ref _consumed);
        }

        public override string ToString()
        {
            return $"{nameof(Produced)}: {Produced}, {nameof(Consumed)}: {Consumed}";
        }
    }
}