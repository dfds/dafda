using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tester
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await CreateHostBuilder(args).Build().RunAsync(tokenSource.Token);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddConsumer(cfg =>
                    {
                        cfg.WithBootstrapServers("localhost:29092");
                        cfg.WithGroupId("foo");
                        cfg.RegisterMessageHandler<PositionMessage, MessageHandler>("p-project.tracking.vehicles", "vehicle_position_changed");
                    });
                    services.AddProducerFor<IHostedService, MessageProducer>(o => 
                    {
                        o.WithBootstrapServers("localhost:29092"); 
                        o.Register<PositionMessage>("p-project.tracking.vehicles", "vehicle_position_changed", k => k.VehicleId); 
                    });
                    services.AddHostedService<MessageProducer>();
                });

    }

    public class PositionMessage
    {
        public string VehicleId { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"vehicle: {VehicleId}, @~{Timestamp.TimeOfDay}";
        }
    }

    public class MessageProducer : IHostedService
    {
        private readonly Producer producer;

        public MessageProducer(Producer producer)
        {
            this.producer = producer;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100, cancellationToken);
                await producer.Produce(new PositionMessage() { Timestamp = DateTime.UtcNow, VehicleId = DateTime.UtcNow.Ticks.ToString() });
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class MessageHandler : IMessageHandler<PositionMessage>
    {
        public Task Handle(PositionMessage message, MessageHandlerContext context)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay}> {message}");
            return Task.CompletedTask;
        }
    }
}
