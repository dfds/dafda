using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Dafda.Consuming;
using Dafda.Outbox;
using Dafda.Serializing;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Dafda.Diagnostics;

internal static class DafdaActivitySource
{
    public static TextMapPropagator Propagator { get; set; } = Propagators.DefaultTextMapPropagator;
    private static readonly AssemblyName AssemblyName = typeof(KafkaConsumerScope).Assembly.GetName();
    private static readonly ActivitySource ActivitySource = new(AssemblyName.Name, AssemblyName.Version.ToString());

    /// <summary>
    /// Starts an activity for receiving a message from a consumer.
    /// </summary>
    /// <param name="carrier">trace context carrier</param>
    /// <returns></returns>
    public static Activity StartReceivingActivity(MessageResult carrier)
    {
        // Extract the context injected in the metadata by the publisher
        var parentContext = Propagator.Extract(default, carrier.Message.Metadata, ExtractContextFromMetadata);

        // Inject extracted info into current context
        Baggage.Current = parentContext.Baggage;

        // Start the activity
        return ActivitySource.StartActivity($"{carrier.Topic} {carrier.Message.Metadata.Type} {OpenTelemetryMessagingOperation.Consumer.Receive}", ActivityKind.Consumer, parentContext.ActivityContext)
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
    /// <param name="carrier">trace context carrier</param>
    /// <returns></returns>
    public static Activity StartPublishingActivity(PayloadDescriptor carrier)
    {
        // Start the activity
        var activity = ActivitySource.StartActivity($"{carrier.TopicName} {carrier.MessageType} {OpenTelemetryMessagingOperation.Producer.Publish}", ActivityKind.Producer)
            .AddDefaultMessagingTags(
                destinationName: carrier.TopicName,
                messageId: carrier.MessageId,
                clientId: carrier.ClientId,
                partitionKey: carrier.PartitionKey)
            .AddProducerMessagingTags();

        // Extract the current activity context
        var contextToInject = activity?.Context
                              ?? default;

        // Inject the current activity context into the message headers
        Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), carrier, InjectContextToPayload);

        return activity;
    }

    public static Activity StartPublishingActivity(OutboxEntry entry, string clientId)
    {
        PropagationContext parentContext = default;
        var messageType = string.Empty;
        var messageId = string.Empty;

        if(!TryDeserializePayload(entry.Payload, out var payload))
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
        var activity = ActivitySource
            .StartActivity($"{entry.Topic} {messageType} {OpenTelemetryMessagingOperation.Producer.Publish}",
                ActivityKind.Producer, parentContext.ActivityContext)
            .AddDefaultMessagingTags(
                destinationName: entry.Topic,
                messageId: messageId,
                clientId: clientId,
                partitionKey: entry.Key)
            .AddProducerMessagingTags();

        // Extract the current activity context
        var contextToInject = activity?.Context
                              ?? default;

        // Inject the current activity context into the message headers
        Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), entry, InjectContextToOutboxEntry);

        return activity;
    }

    /// <summary>
    /// Starts an activity for enqueuing a message to the outbox.
    /// </summary>
    /// <param name="carrier">trace context carrier</param>
    /// <returns></returns>
    public static Activity StartOutboxEnqueueingActivity(Metadata carrier)
    {
        // Start the activity
        var activity = ActivitySource.StartActivity("Outbox message enqueue");

        // Extract the current activity context
        var contextToInject = activity?.Context
                              ?? default;

        // Inject the current activity context into the message headers
        Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), carrier, InjectContextToMetadata);

        return activity;
    }


    private static void InjectContextToOutboxEntry(OutboxEntry outboxEntry, string key, string value)
    {
        if (!TryDeserializePayload(outboxEntry.Payload, out var payload)) return;

        payload[key] = value;
        outboxEntry.Payload = JsonSerializer.Serialize(payload);
    }

    private static void InjectContextToPayload(PayloadDescriptor descriptor, string key, string value)
    {
        descriptor.AddHeader(key, value);
    }

    private static void InjectContextToMetadata(Metadata metadata, string key, string value)
    {
        metadata[key] = value;
    }

    private static IEnumerable<string> ExtractContextFromMetadata(Metadata metadata, string key)
    {
        yield return metadata[key];
    }


    private static IEnumerable<string> ExtractContextFromDictionary(IDictionary<string, string> dictionary, string key)
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            yield return value;
        }
    }

    private static bool TryDeserializePayload(string payload, out Dictionary<string, string> payloadDictionary)
    {
        try
        {
            payloadDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(payload);
            return true;
        }
        catch
        {
            payloadDictionary = null;
            return false;
        }
    }
}