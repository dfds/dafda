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
        services.AddProducerFor<Service>(options =>
        {
            // configuration settings
            options.WithBootstrapServers("localhost:9092");

            // register outgoing messages (includes outbox messages)
            options.Register<Test>("test-topic", "test-event", @event => @event.AggregateId);
        });
    }
}
```

!!! warning "Producers are registered as a per service registration"
    The `Service` is registered with the .NET dependency injection, and able to produce `Test` messages on the Kafka topic `test-topic`.

### Create a message class

Create a POCO representation of the Kafka message:

```csharp
public class Test
{
    public string AggregateId { get; set; }
}
```

### Produce messages

Inject a dependency on `Producer` in the `Service` class and call the `Produce` method:

```csharp
public class Service
{
    private readonly Producer _producer;

    public Service(Producer producer)
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

!!! info "Kafka Producer Settings ([Confluent Docs](https://docs.confluent.io/current/installation/configuration/producer-configs.html))"

### Producer Options

Information about some of basic options are available in the general [Configuration](/configuration) section.

#### Message Registration

It may seem a bit perculiar that outgoing (produced) messages needs to be registered, however, the current version of Dafda opted for a centralized configuration scheme.

In order to produce message, messages needs to be registered for a given producer, like:

```csharp
public class Startup
{
    ...

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // configure messaging: producer
        services.AddProducerFor<Service>(options =>
        {
            // configuration settings
            options.WithBootstrapServers("localhost:9092");

            // register outgoing messages (includes outbox messages)
            options.Register<Test>("test-topic", "test-event", @event => @event.AggregateId);
        });
    }
}
```

The class `Test` is now register for the `Service` producer, and able to send `test-event` message on the Kafka topic `test-topic`. The lambda function at the end of the `Register` call
is used to select the partition key, so that ordering is applicable.

#### Message Id Generator

Each message should contain a unique way of identifying a specific instances of a message. The message id is pass along in the [Message Envelope](/messages/#message-envelope), and consumers should be able to use it for, e.g., deduplication.

The default `MessageIdGenerator` generates a `GUID` for each call, deriving from the `MessageIdGenerator` class and implementing the `NextMessageId` method allows for overriding the default behavior, like:

```csharp
public class Startup
{
    ...

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // configure messaging: producer
        services.AddProducerFor<Service>(options =>
        {
            options.WithMessageIdGenerator(new CustomMessageIdGenerator());
        });
    }
}
```

#### Message Serialization

It some possible to override the default JSON message serialization (here is more on [Message Deserialization](/consumer/#message-deserialization)).

To implement a custom message serializer implement the `IPayloadSerializer` interface:

```csharp
/// <summary>
/// Implementations must use the message payload and metadata in the
/// <see cref="PayloadDescriptor"/> in order to create a string representation.
/// The MIME type format should be described by the <see cref="PayloadFormat"/>
/// property. 
/// </summary>
public interface IPayloadSerializer
{
    /// <summary>
    /// The MIME type of the payload format
    /// </summary>
    string PayloadFormat { get; }

    /// <summary>
    /// Serialize the payload using the message and metadata in the
    /// <see cref="PayloadDescriptor"/>
    /// </summary>
    /// <param name="payloadDescriptor">The payload description</param>
    /// <returns>A string representation of the payload</returns>
    Task<string> Serialize(PayloadDescriptor payloadDescriptor);
}
```

And use the following to replace the default message serialization for __all__ messages:

```csharp
public class Startup
{
    ...

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // configure messaging: producer
        services.AddProducerFor<Service>(options =>
        {
            options.WithDefaultPayloadSerializer(new XmlPayloadSerializer());
        });
    }
}
```

Or the following to replace message serialization for only a specific topic:

```csharp
public class Startup
{
    ...

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // configure messaging: producer
        services.AddProducerFor<Service>(options =>
        {
            options.WithPayloadSerializer("test-topic", new XmlPayloadSerializer());
        });
    }
}
```
