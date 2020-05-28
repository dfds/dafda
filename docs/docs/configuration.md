# Configuration

## Defaults

Dafda relies on the defaults of [`confluent-kafka-dotnet`](https://github.com/confluentinc/confluent-kafka-dotnet) nuget package, which in turn relies on the defaults from [`librdkafka`](https://github.com/edenhill/librdkafka/blob/master/CONFIGURATION.md).

## Basic

For consumers the most basic and _required_ options are `bootstrap.servers` and `group-id`, which can be configured, e.g. in `Startup.cs`, and is usually
enough for local development (see [docker-compose.yml](https://github.com/dfds/dafda/blob/master/docker-compose.yml))

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // configure messaging: consumer
        services.AddConsumer(options =>
        {
            // configuration settings
            options.WithBootstrapServers("localhost:9092");
            options.WithGroupId("consumer-group-id");

            // register message handlers
            ...
        });
    }
}
```

For specific configuration options see [Consumer](/consumer/#configuration), [Producer](/producer/#configuration), and [Outbox](/outbox/#configuration) respectively.

## Manual

!!! warning "Manually added configuration options take precedence over other configuration sources"

Example of manually setting the `auto.offset.reset` to `earliest` in `Startup.cs` to read from the beginning of the topic:

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // configure messaging: consumer
        services.AddConsumer(options =>
        {
            // configuration settings
            options.WithConfiguration("auto.offset.reset", "earliest);
            // other configuration options...

            // register message handlers
            ...
        });
    }
}
```

## Environment

!!! info "Dafda is able to use environment variables to keep configuration minimal and reusable"

Configuring a new self-contained service by gathering configuration from environment variables:

| Name                            | Value                            |
| ------------------------------- | -------------------------------- |
| DEFAULT_KAFKA_BOOTSTRAP_SERVERS | default.kafka.confluent.net:9092 |
| SAMPLE_KAFKA_GROUP_ID           | sample-group-id                  |

!!! warning "Caviat"
    Further configuration values is most likely required in a production setup.


Simply add the following to `Startup.cs`:

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // configure messaging: consumer
        services.AddConsumer(options =>
        {
            // configuration settings
            options.WithConfigurationSource(Configuration);
            options.WithEnvironmentStyle("DEFAULT_KAFKA", "SAMPLE_KAFKA");

            // register message handlers
            ...
        });
    }
}
```

!!! info "Under the hood"
    Dafda will check all environment variables starting with `DEFAULT_KAFKA_` or `SAMPLE_KAFKA_`,
    and match the rest of the environment key against valid Kafka configurations (see [Consumer Configuration](/consumer/#configuration),
    [Producer Configuration](/producer/#configuration) or [Outbox Configuration](/outbox/#configuration) for more info)
     and apply them to the Consumer configuration used by Kafka.

### Conventions

The default naming convention of environment variables is the concatenation of the optionally
supplied prefix and the Kafka configuration key transformed into [screaming snake case](https://en.wikipedia.org/wiki/Snake_case), where
each word is in uppercase and separated by an underscore (`_`). That means `group.id` could be supplied in the environment as `PREFIX_GROUP_ID`.

!!! info "Naming conventions are freely configurable"
    Configuration keys can be transformed using the `WithNamingConvention` method:

    ```csharp
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // configure messaging: consumer
            services.AddConsumer(options =>
            {
                // configuration settings
                options.WithConfigurationSource(Configuration);
                options.WithNamingConvention(key => key.ToLower());

                // register message handlers
                ...
            });
        }
    }

    ```

### Automatic Configuration Keys

Dafda will automatically attempt to look for the following Kafka configuration:

| Key                     | Consumer | Producer |
| ------------------------| :------: | :------: |
| bootstrap.servers       |    x     |    x     |
| group.id                |    x     |   n/a    |
| enable.auto.commit      |    x     |   n/a    |
| broker.version.fallback |    x     |    x     |
| api.version.fallback.ms |    x     |    x     |
| ssl.ca.location         |    x     |    x     |
| sasl.username           |    x     |    x     |
| sasl.password           |    x     |    x     |
| sasl.mechanisms         |    x     |    x     |
| security.protocol       |    x     |    x     |

## Custom

Create a custom implementation of [ConfigurationSource](https://github.com/dfds/dafda/blob/master/src/Dafda/Configuration/ConfigurationSource.cs) and implement the `GetByKey` method:

```csharp
public class CustomConfigurationSource : ConfigurationSource
{
    public override string GetByKey(string key)
    {
        return "some-configuration-value";
    }
}
```

Use the custom implementation in `Startup.cs`:

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // configure messaging: consumer
        services.AddConsumer(options =>
        {
            // configuration settings
            options.WithConfigurationSource(new CustomConfigurationSource());

            // further configurations...
            ...
        });
    }
}
```
