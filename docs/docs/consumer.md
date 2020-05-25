# Consumer

## Quick Start

1. Setup consumer configuration
2. Create a message class
3. Create a message handler

### Setup consumer configuration

Add Kafka consumer configuration and message handlers in `Startup`'s `ConfigureServices()`:

```csharp
public class Startup
{
    ...

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // configure messaging: consumer
        services.AddConsumer(options =>
        {
            // configuration settings
            options.WithBootstrapServers("http://localhost:9092");
            options.WithGroupId("consumer-group-id");

            // register message handlers
            options.RegisterMessageHandler<Test, TestHandler>("test-topic", "test-event");
        });
    }

    ...
}
```

!!! note "Multiple consumers"
    It is possible to add multiple consumers (with different configuration) using the `AddConsumer()` extension method.

### Create a message class

Create a POCO representation of the Kafka message:

```csharp
public class Test
{
    public string AggregateId { get; set; }
}
```

### Create a message handler

Create a message handler that implements the [`IMessageHandler<Test>`](https://github.com/dfds/dafda/blob/master/src/Dafda/Consuming/IMessageHandler.cs) interface, and use the `Handle()` method to do all the needed business logic to handle the message:

```csharp
public class TestHandler : IMessageHandler<Test>
{
    private readonly ILogger<TestHandler> _logger;

    public TestHandler(ILogger<TestHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(Test message, MessageHandlerContext context)
    {
        _logger.LogInformation(@"Handled: {@Message}", message);

        return Task.CompletedTask;
    }
}
```

!!! tip "MessageHandlerContext"
    The `MessageHandlerContext` contains information about the message, such as:
    
    * Message Identifier
    * Correlation Identifier
    * Message Type
    * User-defined key/value pairs

## Dependency Injection

Types registered in the `IServiceCollection` are available as constructor arguments. The `IMessageHandler<T>` implementation and any dependencies are resolved as part of the same scope, and disposed at the end of the `Handle()` method invocation.
    
!!! tip "DbContext"
    Therefore, it is entirely possible to resolve `DbContext` instances and have them behave as expected, however, **transaction management and calls to `SaveChangesAsync()` are left up to the user**.

!!! warning "Service Lifetimes"
    As always, with dependency injection, be mindful of [service lifetimes](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1#service-lifetimes).

## Configuration

| Key | Required |
|-|-|
| bootstrap.servers | true |
| group.id | true |
| enable.auto.commit | false |

[Consumer Configurations](https://docs.confluent.io/current/installation/configuration/consumer-configs.html)
