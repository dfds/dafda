using System;
using System.Threading.Tasks;
using Dafda.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace OutboxProcessor
{
    public static class Program
    {
        private const string ApplicationName = "Sample.Outbox.Processor";

        public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Dafda", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: $"{ApplicationName.ToUpper()} [{{Timestamp:HH:mm:ss}} {{Level:u3}}] {{Message:lj}}{{NewLine}}{{Exception}}")
                .CreateLogger();

            try
            {
                Log.Information($"Starting {ApplicationName} application");

                await CreateHostBuilder()
                    .Build()
                    .RunAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"{ApplicationName} application terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return new HostBuilder()
                .UseSerilog()
                .ConfigureHostConfiguration(config => { config.AddEnvironmentVariables(); })
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;

                    // configure persistence (Postgres)
                    var connectionString = configuration["SAMPLE_OUTBOX_PROCESSOR_CONNECTION_STRING"];

                    var outboxNotification = new PostgresListener(connectionString, "dafda_outbox", TimeSpan.FromSeconds(30));

                    services.AddSingleton(provider => outboxNotification); // register to dispose

                    services.AddOutboxProducer(options =>
                    {
                        // configuration settings
                        options.WithConfigurationSource(configuration);
                        options.WithEnvironmentStyle("DEFAULT_KAFKA");

                        // include outbox (polling publisher)
                        options.WithUnitOfWorkFactory(_ => new OutboxUnitOfWorkFactory(connectionString));
                        options.WithListener(outboxNotification);
                    });
                });
        }
    }
}