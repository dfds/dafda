# Configuration

## Defaults

Dafda relies on the default of [`Confluent.Kafka`](https://github.com/confluentinc/confluent-kafka-dotnet), which in turn relies on the default from [`librdkafka`](https://github.com/edenhill/librdkafka/blob/master/CONFIGURATION.md).

## Basic

```csharp
// configure messaging: consumer
services.AddConsumer(options =>
{
    // configuration settings
    options.WithBootstrapServers("http://localhost:9092");
    options.WithGroupId("consumer-group-id");

    // register message handlers
    ...
});
```

## Environment

Consider the following environment variables:

| Name                            | Value                                    |
|---------------------------------|------------------------------------------|
| DEFAULT_KAFKA_BOOTSTRAP_SERVERS | https://default.kafka.confluent.net:9092 |
| SAMPLE_KAFKA_GROUP_ID           | sample-group-id                          |

The following:

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // configure messaging: consumer
    services.AddConsumer(options =>
    {
        // configuration settings
        options.WithConfigurationSource(Configuration);
        options.WithEnvironmentStyle("DEFAULT_KAFKA");
        options.WithEnvironmentStyle("SAMPLE_KAFKA");

        // register message handlers
        ...
    });
}
```

will take all environment variables starting with either `DEFAULT_KAFKA_` or `SAMPLE_KAFKA_` and put them into the configuration used by Consumer.
