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