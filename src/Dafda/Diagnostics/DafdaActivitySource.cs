using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dafda.Consuming;
using Dafda.Outbox;
using Dafda.Serializing;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Dafda.Diagnostics;

/// <summary>
/// Provides methods to create and support OpenTelemetry activities for various Dafda operations.
/// Activity names are based on https://github.com/open-telemetry/semantic-conventions/blob/v1.24.0/docs/messaging/messaging-spans.md
/// </summary>
internal static class DafdaActivitySource
{
    /// <summary>
    /// Gets or sets the <see cref="TextMapPropagator"/> for injecting and extracting trace context.
    /// </summary>
    public static TextMapPropagator Propagator { get; set; } = Propagators.DefaultTextMapPropagator;

    private static readonly AssemblyName AssemblyName = typeof(KafkaConsumerScope).Assembly.GetName();
    private static readonly ActivitySource ActivitySource = new(AssemblyName.Name, AssemblyName.Version.ToString());

    private static readonly JsonSerializerOptions DafdaJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Starts an activity for receiving a message from a consumer.
    /// </summary>
    /// <param name="carrier">The message result containing the trace context carrier.</param>
    /// <returns>The started <see cref="Activity"/> for the receiving message operation.</returns>
    public static Activity StartReceivingActivity(MessageResult carrier)
    {
        // Extract the context injected in the metadata by the publisher
        var parentContext = Propagator.Extract(default, carrier.Message.Metadata, ExtractContextFromMetadata);

        // Inject extracted info into current context
        Baggage.Current = parentContext.Baggage;

        // Start the activity
        var activityName = $"{carrier.Topic} {carrier.Message.Metadata.Type} {OpenTelemetryMessagingOperation.Consumer.Receive}";
        return ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext.ActivityContext)
            .AddDefaultMessagingTags(
                destinationName: carrier.Topic,
                messageId: carrier.Message.Metadata.MessageId,
                clientId: carrier.ClientId,
                partitionKey: carrier.PartitionKey,
                partition: carrier.Partition)
            .AddConsumerMessagingTags(
                groupId: carrier.GroupId);
    }

    /// <summary>
    /// Starts an activity for publishing a message to a producer.
    /// </summary>
    /// <param name="carrier">The payload descriptor containing the trace context carrier.</param>
    /// <returns>The started <see cref="Activity"/> for the publishing message operation.</returns>
    public static Activity StartPublishingActivity(PayloadDescriptor carrier)
    {
        // Start the activity
        var activityName = $"{carrier.TopicName} {carrier.MessageType} {OpenTelemetryMessagingOperation.Producer.Publish}";
        var activity = ActivitySource.StartActivity(activityName, ActivityKind.Producer)
            .AddDefaultMessagingTags(
                destinationName: carrier.TopicName,
                messageId: carrier.MessageId,
                clientId: carrier.ClientId,
                partitionKey: carrier.PartitionKey)
            .AddProducerMessagingTags();

        // Extract the current activity context
        var contextToInject = activity?.Context ?? default;

        // Inject the current activity context into the message headers
        Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), carrier, InjectContextToPayload);

        return activity;
    }

    /// <summary>
    /// Starts an activity for publishing an outbox entry to a producer.
    /// </summary>
    /// <param name="entry">The outbox entry containing the trace context carrier.</param>
    /// <param name="clientId">The client ID associated with the outbox entry.</param>
    /// <returns>The started <see cref="Activity"/> for the publishing outbox entry operation, or <c>null</c> if deserialization fails.</returns>
    public static Activity StartPublishingActivity(OutboxEntry entry, string clientId)
    {
        PropagationContext parentContext = default;
        var messageType = string.Empty;
        var messageId = string.Empty;

        if (!TryDeserializePayload(entry.Payload, out var payload))
        {
            return null;
        }
        
        // extract message type
        payload.TryGetValue("type", out messageType);
        payload.TryGetValue("messageId", out messageId);

        // Extract the context injected in the metadata by the publisher
        parentContext = Propagator.Extract(default, payload, ExtractContextFromDictionary);

        // Inject extracted info into current context
        Baggage.Current = parentContext.Baggage;

        // Start the activity
        var activityName = $"{entry.Topic} {messageType} {OpenTelemetryMessagingOperation.Producer.Publish}";
        var activity = ActivitySource.StartActivity(activityName, ActivityKind.Producer, parentContext.ActivityContext)
            .AddDefaultMessagingTags(
                destinationName: entry.Topic,
                messageId: messageId,
                clientId: clientId,
                partitionKey: entry.Key)
            .AddProducerMessagingTags();

        // Extract the current activity context
        var contextToInject = activity?.Context ?? default;

        // Inject the current activity context into the message headers
        Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), entry, InjectContextToOutboxEntry);

        return activity;
    }

    /// <summary>
    /// Starts an activity for enqueuing a message to the outbox.
    /// </summary>
    /// <param name="carrier">The metadata containing the trace context carrier.</param>
    /// <returns>The started <see cref="Activity"/> for the outbox enqueueing operation.</returns>
    public static Activity StartOutboxEnqueueingActivity(Metadata carrier)
    {
        // Start the activity
        var activityName = "Outbox enqueue";
        var activity = ActivitySource.StartActivity(activityName);

        // Extract the current activity context
        var contextToInject = activity?.Context ?? default;

        // Inject the current activity context into the message headers
        Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), carrier, InjectContextToMetadata);

        return activity;
    }

    /// <summary>
    /// Starts an activity for creating an outbox entry.
    /// </summary>
    /// <param name="payloadDescriptor">The payload descriptor containing information about the payload.</param>
    /// <param name="metadata">The metadata containing the trace context carrier.</param>
    /// <returns>The started <see cref="Activity"/> for the outbox entry creation operation.</returns>
    public static Activity StartOutboxEntryCreationActivity(PayloadDescriptor payloadDescriptor, Metadata metadata)
    {
        // Start the activity
        var activityName = $"{payloadDescriptor.TopicName} {payloadDescriptor.MessageType} {OpenTelemetryMessagingOperation.Producer.Create}";
        var activity = ActivitySource.StartActivity(activityName, ActivityKind.Internal)
            .AddDefaultMessagingTags(
                destinationName: payloadDescriptor.TopicName,
                messageId: payloadDescriptor.MessageId,
                clientId: payloadDescriptor.ClientId,
                partitionKey: payloadDescriptor.PartitionKey)
            .AddProducerMessagingTags();

        // Extract the current activity context
        var contextToInject = activity?.Context ?? default;
        
        // Inject the current activity context into the message headers
        Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), metadata, InjectContextToMetadata);

        return activity;
    }

    /// <summary>
    /// Injects the trace context into the outbox entry.
    /// </summary>
    /// <param name="outboxEntry">The outbox entry to inject context into.</param>
    /// <param name="key">The key for the trace context.</param>
    /// <param name="value">The value for the trace context.</param>
    private static void InjectContextToOutboxEntry(OutboxEntry outboxEntry, string key, string value)
    {
        if (!TryDeserializePayload(outboxEntry.Payload, out var payload)) return;

        payload[key] = value;
        outboxEntry.Payload = JsonSerializer.Serialize(payload);
    }

    /// <summary>
    /// Injects the trace context into the payload descriptor.
    /// </summary>
    /// <param name="descriptor">The payload descriptor to inject context into.</param>
    /// <param name="key">The key for the trace context.</param>
    /// <param name="value">The value for the trace context.</param>
    private static void InjectContextToPayload(PayloadDescriptor descriptor, string key, string value)
    {
        descriptor.AddHeader(key, value);
    }

    /// <summary>
    /// Injects the trace context into the metadata.
    /// </summary>
    /// <param name="metadata">The metadata to inject context into.</param>
    /// <param name="key">The key for the trace context.</param>
    /// <param name="value">The value for the trace context.</param>
    private static void InjectContextToMetadata(Metadata metadata, string key, string value)
    {
        metadata[key] = value;
    }

    /// <summary>
    /// Extracts the trace context from the metadata.
    /// </summary>
    /// <param name="metadata">The metadata containing the trace context.</param>
    /// <param name="key">The key for the trace context.</param>
    /// <returns>An enumerable collection of trace context values.</returns>
    private static IEnumerable<string> ExtractContextFromMetadata(Metadata metadata, string key)
    {
        yield return metadata[key];
    }

    /// <summary>
    /// Extracts the trace context from the dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary containing the trace context.</param>
    /// <param name="key">The key for the trace context.</param>
    /// <returns>An enumerable collection of trace context values.</returns>
    private static IEnumerable<string> ExtractContextFromDictionary(IDictionary<string, string> dictionary, string key)
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            yield return value;
        }
    }

    /// <summary>
    /// Tries to deserialize the payload string into a dictionary.
    /// </summary>
    /// <param name="payload">The payload string to deserialize.</param>
    /// <param name="payloadDictionary">The resulting dictionary after deserialization.</param>
    /// <returns><c>true</c> if deserialization is successful; otherwise, <c>false</c>.</returns>
    public static bool TryDeserializePayload(string payload, out Dictionary<string, string> payloadDictionary)
    {
        try
        {
            var intermediateResult = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payload, DafdaJsonSerializerOptions);
            payloadDictionary = new Dictionary<string, string>();

            foreach (var kvp in intermediateResult)
            {
                switch (kvp.Value.ValueKind)
                {
                    case JsonValueKind.String:
                        payloadDictionary[kvp.Key] = kvp.Value.GetString();
                        break;
                    case JsonValueKind.Number:
                        payloadDictionary[kvp.Key] = kvp.Value.GetRawText(); // Keeps the original number representation as a string
                        break;
                    default:
                        payloadDictionary[kvp.Key] = kvp.Value.ToString();
                        break;
                }
            }
            return true;
        }
        catch
        {
            payloadDictionary = null;
            return false;
        }
    }
}