using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Confluent.Kafka;
using Dafda.Consuming;
using Dafda.Producing;
using Dafda.Serializing;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Metadata = Dafda.Consuming.Metadata;

namespace Dafda.Diagnostics;

/// <summary>
/// Provides the OpenTelemetry messaging attributes.
/// The complete list of messaging attributes specification is available here: https://opentelemetry.io/docs/specs/semconv/messaging/messaging-spans/#messaging-attributes
/// </summary>
internal static class OpenTelemetryMessagingAttributes
{
    /// <summary>
    /// Message system. For Kafka, attribute value must be "kafka".
    /// </summary>
    public const string System = "messaging.system";

    /// <summary>
    /// Message destination. For Kafka, attribute value must be a Kafka topic.
    /// </summary>
    public const string Destination = "messaging.destination";

    /// <summary>
    /// Destination kind. For Kafka, attribute value must be "topic".
    /// </summary>
    public const string DestinationKind = "messaging.destination_kind";

    /// <summary>
    /// A value used by the messaging system as an identifier for the message, represented as a string.
    /// </summary>
    public const string MessageId = "messaging.message.id";

    /// <summary>
    /// A string identifying the kind of messaging operation
    /// </summary>
    public const string Operation = "messaging.operation";

    /// <summary>
    /// A unique identifier for the client that consumes or produces a message.
    /// </summary>
    public const string ClientId = "messaging.client_id";

    /// <summary>
    /// Kafka partition number.
    /// </summary>
    public const string KafkaPartition = "messaging.kafka.destination.partition";

    /// <summary>
    /// Kafka message key.
    /// </summary>
    public const string KafkaMessageKey = "messaging.kafka.message_key";

    /// <summary>
    /// Name of the Kafka Consumer Group that is handling the message. Only applies to consumers, not producers.
    /// </summary>
    public const string KafkaConsumerGroup = "messaging.kafka.consumer.group";
}

internal static class ConsumerActivitySource
{
    private const string ActivityNameSuffix = "receive";

    private static readonly AssemblyName AssemblyName = typeof(KafkaConsumerScope).Assembly.GetName();
    private static readonly ActivitySource ActivitySource = new(AssemblyName.Name, AssemblyName.Version.ToString());
    public static TextMapPropagator Propagator { get; set; } = Propagators.DefaultTextMapPropagator;

    public static Activity StartActivity(MessageResult @event)
    {
        // Extract the context injected in the metadata by the publisher
        var message = @event.Message;
        var parentContext = Propagator.Extract(default, message.Metadata, ExtractFromMetadata);

        // Inject extracted info into current context
        Baggage.Current = parentContext.Baggage;

        // Start the activity
        return ActivitySource.StartActivity($"{@event.Topic} {ActivityNameSuffix}", ActivityKind.Consumer, parentContext.ActivityContext)
            .AddDefaultOpenTelemetryTags(
                topicName: @event.Topic,
                messageId: @event.Message.Metadata.MessageId,
                clientId: @event.ClientId,
                partitionKey: @event.PartitionKey,
                partition: @event.Partition)
            .AddConsumerOpenTelemetryTags(
                groupId: @event.GroupId);
    }

    private static IEnumerable<string> ExtractFromMetadata(Metadata metadata, string key)
    {
        yield return metadata[key];
    }
}

internal static class ProducerActivitySource
{
    private const string ActivityNameSuffix = "send";
    private static readonly AssemblyName AssemblyName = typeof(KafkaProducer).Assembly.GetName();
    private static readonly ActivitySource ActivitySource = new(AssemblyName.Name, AssemblyName.Version.ToString());
    public static TextMapPropagator Propagator { get; set; } = Propagators.DefaultTextMapPropagator;

    public static Activity StartActivity(PayloadDescriptor payloadDescriptor)
    {
        // Extract the current activity context
        var contextToInject = Activity.Current?.Context
                              ?? default;

        // Inject the current context into the message headers
        Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), payloadDescriptor, InjectTraceContext);

        // Start the activity
        return ActivitySource.StartActivity($"{payloadDescriptor.TopicName} {ActivityNameSuffix}", ActivityKind.Producer)
            .AddDefaultOpenTelemetryTags(
                topicName: payloadDescriptor.TopicName,
                messageId: payloadDescriptor.MessageId,
                clientId: payloadDescriptor.ClientId,
                partitionKey: payloadDescriptor.PartitionKey)
            .AddProducerOpenTelemetryTags();
    }

    private static void InjectTraceContext(PayloadDescriptor descriptor, string key, string value)
    {
        descriptor.AddHeader(key, value);
    }
}