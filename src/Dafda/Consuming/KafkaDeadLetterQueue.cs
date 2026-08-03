namespace Dafda.Consuming;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

/// <summary>
/// Publishes failed messages to a Kafka dead letter topic. When no explicit
/// topic name is configured, the target topic is derived from the source
/// topic of the message using the <see cref="DefaultTopicSuffix"/>.
/// </summary>
internal sealed class KafkaDeadLetterQueue(
    ILoggerFactory loggerFactory,
    IEnumerable<KeyValuePair<string, string>> configuration,
    string topicName)
    : IDeadLetterQueue, IDisposable
{
    private const string DefaultTopicSuffix = ".dead-letter";

    private readonly ILogger<KafkaDeadLetterQueue> _logger = loggerFactory.CreateLogger<KafkaDeadLetterQueue>();
    private readonly IProducer<string, string> _producer = new ProducerBuilder<string, string>(configuration).Build();

    private string ResolveTopic(MessageResult message)
    {
        return ResolveTopicName(topicName, message.Topic);
    }

    internal static string ResolveTopicName(string configuredTopicName, string sourceTopic)
    {
        return configuredTopicName ?? $"{sourceTopic}{DefaultTopicSuffix}";
    }

    public async Task Send(MessageResult message, Exception exception, CancellationToken cancellationToken)
    {
        var topic = ResolveTopic(message);

        _logger.LogWarning(
            exception,
            "Dead-lettering message with key {Key} from topic {SourceTopic} to {DeadLetterTopic}",
            message.PartitionKey,
            message.Topic,
            topic);

        var headers = new Headers
        {
            { "dafda-dead-letter-source-topic", Encode(message.Topic) },
            { "dafda-dead-letter-exception-type", Encode(exception.GetType().FullName) },
            { "dafda-dead-letter-exception-message", Encode(exception.Message) },
            { "dafda-dead-letter-timestamp", Encode(DateTimeOffset.UtcNow.ToString("O")) },
        };

        await _producer.ProduceAsync(
            topic: topic,
            message: new Message<string, string>
            {
                Key = message.PartitionKey,
                Value = message.RawMessage,
                Headers = headers
            },
            cancellationToken);
    }

    private static byte[] Encode(string value)
    {
        return Encoding.UTF8.GetBytes(value ?? string.Empty);
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}