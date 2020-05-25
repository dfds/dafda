# Configuration

## Defaults

Dafda relies on the defaults of [`confluent-kafka-dotnet`](https://github.com/confluentinc/confluent-kafka-dotnet) nuget package, which in turn relies on the defaults from [`librdkafka`](https://github.com/edenhill/librdkafka/blob/master/CONFIGURATION.md).

## Basic

```csharp
// configure messaging: consumer
services.AddConsumer(options =>
{
    // configuration settings
    options.WithBootstrapServers("localhost:9092");
    options.WithGroupId("consumer-group-id");

    // register message handlers
    ...
});
```

## Environment

Consider the following environment variables:

| Name                            | Value                            |
| ------------------------------- | -------------------------------- |
| DEFAULT_KAFKA_BOOTSTRAP_SERVERS | default.kafka.confluent.net:9092 |
| SAMPLE_KAFKA_GROUP_ID           | sample-group-id                  |

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

will take all environment variables starting with `DEFAULT_KAFKA_` or `SAMPLE_KAFKA_`, and match the rest of the environment key against valid Kafka configurations (see [Consumer Configuration](/consumer/#configuration) or [Producer Configuration](/producer/#configuration)), and put them into the configuration used by Consumer.
