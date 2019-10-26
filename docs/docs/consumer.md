### Consumer

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
}
```

Create a POCO representation of the Kafka message:

```csharp
public class Test
{
    public string AggregateId { get; set; }
}
```

Create a message handler:

```csharp
public class TestHandler : IMessageHandler<Test>
{
    private readonly ILogger<TestHandler> _logger;

    public TestHandler(ILogger<TestHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(Test message)
    {
        _logger.LogInformation(@"Handled: {@Message}", message);
        
        return Task.CompletedTask;
    }
}
```
