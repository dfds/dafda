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
    /// A string identifying the kind of messaging operation
    /// </summary>
    public const string Operation = "messaging.operation";

    /// <summary>
    /// A unique identifier for the client that consumes or produces a message.
    /// </summary>
    public const string ClientId = "messaging.client_id";

    /// <summary>
    /// A value used by the messaging system as an identifier for the message, represented as a string.
    /// </summary>
    public const string MessageId = "messaging.message.id";

    /// <summary>
    /// Message destination. For Kafka, attribute value must be a Kafka topic.
    /// </summary>
    public const string DestinationName = "messaging.destination.name";

    /// <summary>
    /// Kafka specific attributes.
    /// </summary>
    internal static class Kafka
    {
        /// <summary>
        /// Partition (int) the message is sent to.
        /// </summary>
        public const string Partition = "messaging.kafka.destination.partition";

        /// <summary>
        /// Name of the Kafka Consumer Group that is handling the message. Only applies to consumers, not producers.
        /// </summary>
        public const string ConsumerGroup = "messaging.kafka.consumer.group";

        /// <summary>
        /// Kafka message key. Message keys in Kafka are used for grouping alike messages to ensure they’re processed on the same partition. They differ from messaging.message.id in that they’re not unique
        /// </summary>
        public const string MessageKey = "messaging.kafka.message.key";
    }
}