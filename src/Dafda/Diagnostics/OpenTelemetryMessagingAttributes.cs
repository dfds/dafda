using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Dafda.Consuming;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

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
    public const string SYSTEM = "messaging.system";

    /// <summary>
    /// Message destination. For Kafka, attribute value must be a Kafka topic.
    /// </summary>
    public const string DESTINATION = "messaging.destination";

    /// <summary>
    /// Destination kind. For Kafka, attribute value must be "topic".
    /// </summary>
    public const string DESTINATION_KIND = "messaging.destination_kind";

    /// <summary>
    /// A value used by the messaging system as an identifier for the message, represented as a string.
    /// </summary>
    public const string MESSAGE_ID = "messaging.message.id";

    /// <summary>
    /// A string identifying the kind of messaging operation
    /// </summary>
    public const string OPERATION = "messaging.operation";

    /// <summary>
    /// The conversation ID identifying the conversation to which the message belongs, represented as a string. Sometimes called “Correlation ID”.
    /// </summary>
    public const string CONVERSATION_ID = "messaging.message.conversation_id";

    /// <summary>
    /// A unique identifier for the client that consumes or produces a message.
    /// </summary>
    public const string CLIENT_ID = "messaging.client_id";

    /// <summary>
    /// Kafka partition number.
    /// </summary>
    public const string KAFKA_PARTITION = "messaging.kafka.destination.partition";

    /// <summary>
    /// Kafka message key.
    /// </summary>
    public const string KAFKA_MESSAGE_KEY = "messaging.kafka.message_key";

    /// <summary>
    /// Name of the Kafka Consumer Group that is handling the message. Only applies to consumers, not producers.
    /// </summary>
    public const string KAFKA_CONSUMER_GROUP = "messaging.kafka.consumer.group";
}

internal static class OpenTelemetryActivityExtensions
{
    private static class Messaging
    {
        public const string System = "kafka";
        public const string DestinationKind = "topic";
    }

    private static class Operation
    {
        public const string Receive = "receive";
        public const string Publish = "publish";
    }

    public static Activity AddDefaultOpenTelemetryTags(this Activity activity,
        string topicName,
        string messageId,
        string conversationId,
        string clientId,
        string partitionKey,
        int partition)
    {
        // messaging tags
        activity.SetTag(OpenTelemetryMessagingAttributes.SYSTEM, Messaging.System);
        activity.SetTag(OpenTelemetryMessagingAttributes.DESTINATION_KIND, Messaging.DestinationKind);

        activity.SetTag(OpenTelemetryMessagingAttributes.DESTINATION, topicName);
        activity.SetTag(OpenTelemetryMessagingAttributes.MESSAGE_ID, messageId);
        activity.SetTag(OpenTelemetryMessagingAttributes.CONVERSATION_ID, conversationId);
        activity.SetTag(OpenTelemetryMessagingAttributes.CLIENT_ID, clientId);

        // kafka specific tags
        activity.SetTag(OpenTelemetryMessagingAttributes.KAFKA_MESSAGE_KEY, partitionKey);
        activity.SetTag(OpenTelemetryMessagingAttributes.KAFKA_PARTITION, partition);

        return activity;
    }

    public static Activity AddConsumerOpenTelemetryTags(this Activity activity, string groupId)
    {
        activity.SetTag(OpenTelemetryMessagingAttributes.KAFKA_CONSUMER_GROUP, groupId);
        activity.SetTag(OpenTelemetryMessagingAttributes.OPERATION, Operation.Receive);
        return activity;
    }

    public static Activity AddProducerOpenTelemetryTags(this Activity activity)
    {
        activity.SetTag(OpenTelemetryMessagingAttributes.OPERATION, Operation.Publish);
        return activity;
    }
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
                conversationId: @event.Message.Metadata.CorrelationId,
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