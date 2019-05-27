using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
using Dafda.Messaging;

namespace Tester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var subscriber = CreateSubscriber();
            var configuration = new ConfigurationBuilder()
                .WithConfiguration("bootstrap.servers", "localhost:29092")
                .WithConfiguration("group.id", "foo")
                .Build();

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            Console.WriteLine("subscribing...");
            await subscriber.Start(configuration,new[] {"p-project.tracking.vehicles"}, tokenSource.Token);
            Console.WriteLine("Stopped!");
        }

        private static TopicSubscriber CreateSubscriber()
        {
            var factory = new ConsumerFactory();

            var handlerRegistry = new MessageHandlerRegistry();
            handlerRegistry.Register<PositionMessage, MessageHandler>("p-project.tracking.vehicles", "vehicle_position_changed");

            var typeResolver = new HandRolledResolver();
            var dispatcher = new LocalMessageDispatcher(handlerRegistry, typeResolver);

            return new TopicSubscriber(factory, dispatcher);
        }
    }

    public class HandRolledResolver : ITypeResolver
    {
        public object Resolve(Type instanceType)
        {
            return Activator.CreateInstance(instanceType);
        }
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
    
    public class MessageHandler : IMessageHandler<PositionMessage>
    {
        public Task Handle(PositionMessage message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay}> {message}");
            return Task.CompletedTask;
        }
    }
}
