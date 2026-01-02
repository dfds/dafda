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
    It is possible to add multiple consumers (with different configuration) using the `AddConsumer()` extension method, which
    might be helpful when consuming from different topic with a different `auto.offset.reset`.

### Create a message class

Create a POCO representation of the Kafka message:

```csharp
public class Test
{
    public string AggregateId { get; set; }
}
```

!!! warning "Private/public properties"
    Due to the nature of `System.Text.Json` having `private` setters will result in properties being skipped by the default deserializer.

!!! warning "Constructors"
    Please use an `public` parameterless constructor to be on the safe side of `System.Text.Json`.

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

    public Task Handle(Test message, MessageHandlerContext context, CancellationToken cancellationToken)
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
    * Message Headers

## Dependency Injection

Types registered in the `IServiceCollection` are available as constructor arguments. The `IMessageHandler<T>` implementation and any dependencies are resolved as part of the same scope, and disposed at the end of the `Handle()` method invocation.
    
!!! tip "DbContext"
    Therefore, it is entirely possible to resolve `DbContext` instances and have them behave as expected, however, **transaction management and calls to `SaveChangesAsync()` are left up to the user**.

!!! warning "Service Lifetimes"
    As always, with dependency injection, be mindful of [service lifetimes](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1#service-lifetimes).

## Configuration

!!! info "Kafka Consumer Settings ([Confluent Docs](https://docs.confluent.io/current/installation/configuration/consumer-configs.html))"

### Consumer Options

Information about some of basic options are available in the general [Configuration](/configuration) section.

#### Message Handler Registration

Adding message handlers during service configuration:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // configure messaging: producer
    services.AddConsumer(options =>
    {
        // register outgoing messages (includes outbox messages)
        options.RegisterMessageHandler<Test, TestHandler>("test-topic", "test-event");

    });
}
```

Will ensure that all messages with the type[^1] `test-event` on the Kafka topic named `test-topic` will be deserialized as an instance of the POCO `Test` and handed to a [transiently](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1#service-lifetimes) resolved instance of `TestHandler`. This is all handled by the .NET Core dependency injection, and Dafda clients need only concern themselves with creating simple messages and matching message handlers.

#### Unconfigured Messages

By default, a consumer will throw a `MissingMessageHandlerRegistrationException` if it receives a message where the type has not been configured with a handler. This can be overridden by providing a different `IUnconfiguredMessageHandlingStrategy`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // configure messaging: producer
    services.AddConsumer(options =>
    {
        // register outgoing messages (includes outbox messages)
        options.RegisterMessageHandler<Test, TestHandler>("test-topic", "test-event");

        // log, but perform no other action for other messages
        options.WithUnconfiguredMessageHandlingStrategy<UseNoOpHandler>();
    });
}
```

`UseNoOpHandler` is built in, and uses an `ILogger` to log an information message about having received the message and then considers it processed.



#### Message Deserialization

In order to gain controler over the deserialization of the message handled by Dafda use `WithIncomingMessageFactory`, like:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // configure messaging: producer
    services.AddConsumer(options =>
    {
        // register outgoing messages (includes outbox messages)
        options.WithIncomingMessageFactory(new XmlMessageSerializer());

    });
}
```

To override the default JSON deserializer, and supply a custom implementation of the `IIncomingMessageFactory` interface.
    
#### Unit of Work Factory

It is possible to override the default Unit of Work behavior for each consumed message, using configuration options and custom implementations of `IHandlerUnitOfWorkFactory` and `IHandlerUnitOfWork`. However, the default implementation adheres to the [scoped service lifetime](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1#service-lifetimes) of .NET Core's dependency injection, allowing it to work in tandem with the scopes of, e.g., EF Core.

!!! example "For more information see [ServiceProviderUnitOfWorkFactory.cs](https://github.com/dfds/dafda/blob/master/src/Dafda/Consuming/ServiceProviderUnitOfWorkFactory.cs)"

[^1]: The message type is part of the [Message Envelope](/messages/#message-envelope)

#### Message Handler Execution Strategy

It is possible to override the default execution strategy used by the consumer to execute message handlers, by providing a custom implementation of `IMessageHandlerExecutionStrategy`.
The message handler execution strategy has the same lifetime as the consumer itself (singleton).

This could for instance allow to execute the message handling inside a resilience pipeline for the given consumer.

Example:
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddConsumer(options =>
    {
        options.WithMessageHandlerExecutionStrategyFactory(sp =>
        {
            return new MyResilientExecutionStrategy();
        });
    });
}
```

Here the `MyResilientExecutionStrategy` is a custom implementation of `IMessageHandlerExecutionStrategy`. The `sp` parameter is the service provider, which can be used to resolve dependencies needed by the execution strategy.


#### Read from beginning

When a consumer starts  Dafda continues consuming from the last committed offset on each topic, if a committed offset exists. 
To read all topics from the beginning of all partitions, use `ReadFromBeginning`: 

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // configure messaging: producer
    services.AddConsumer(options =>
    {
        options.ReadFromBeginning();
    });
}
```
