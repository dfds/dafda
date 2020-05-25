# Producer

## Quick Start

1. Setup producer configuration
2. Create a message class
3. Produce messages

### Setup producer configuration

Add Kafka producer configuration and outgoing messages:

```csharp
public class Startup
{
    ...

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // configure messaging: producer
        services.AddProducer(options =>
        {
            // configuration settings
            options.WithBootstrapServers("localhost:9092");

            // register outgoing messages (includes outbox messages)
            options.Register<Test>("test-topic", "test-event", @event => @event.AggregateId);
        });
    }
}
```

### Create a message class

Create a POCO representation of the Kafka message:

```csharp
public class Test
{
    public string AggregateId { get; set; }
}
```

### Produce messages

Take a dependency on `IProducer` and call the `Produce` method:

```csharp
public class Service
{
    private readonly IProducer _producer;

    public Service(IProducer producer)
    {
        _producer = producer;
    }

    public async Task DoStuff()
    {
        ...
        await _producer.Produce(new Test { AggregateId = "aggregate-id" });
        ...
    }
}
```

## Configuration

| Key | Required |
|-|-|
| bootstrap.servers | true |
| group.id | true |
| enable.auto.commit | false |

[Producer Configurations](https://docs.confluent.io/current/installation/configuration/producer-configs.html)
