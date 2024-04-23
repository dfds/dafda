using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Dafda.Consuming;
using Dafda.Serializing;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Dafda.Diagnostics;

internal static class DafdaActivitySource
{
    public static TextMapPropagator Propagator { get; set; } = Propagators.DefaultTextMapPropagator;
    private static readonly AssemblyName AssemblyName = typeof(KafkaConsumerScope).Assembly.GetName();
    private static readonly ActivitySource ActivitySource = new(AssemblyName.Name, AssemblyName.Version.ToString());

    public static Activity StartReceivingActivity(MessageResult @event)
    {
        // Extract the context injected in the metadata by the publisher
        var parentContext = Propagator.Extract(default, @event.Message.Metadata, ExtractFromMetadata);

        // Inject extracted info into current context
        Baggage.Current = parentContext.Baggage;

        // Start the activity
        return ActivitySource.StartActivity($"{@event.Topic} {@event.Message.Metadata.Type} {OpenTelemetryMessagingOperation.Consumer.Receive}", ActivityKind.Consumer, parentContext.ActivityContext)
            .AddDefaultMessagingTags(
                destinationName: @event.Topic,
                messageId: @event.Message.Metadata.MessageId,
                clientId: @event.ClientId,
                partitionKey: @event.PartitionKey,
                partition: @event.Partition)
            .AddConsumerMessagingTags(
                groupId: @event.GroupId);
    }

    public static Activity StartPublishingActivity(PayloadDescriptor payloadDescriptor)
    {
        // Start the activity
        var activity = ActivitySource.StartActivity($"{payloadDescriptor.TopicName} {payloadDescriptor.MessageType} {OpenTelemetryMessagingOperation.Producer.Publish}", ActivityKind.Producer)
            .AddDefaultMessagingTags(
                destinationName: payloadDescriptor.TopicName,
                messageId: payloadDescriptor.MessageId,
                clientId: payloadDescriptor.ClientId,
                partitionKey: payloadDescriptor.PartitionKey)
            .AddProducerMessagingTags();

        // Extract the current activity context
        var contextToInject = activity?.Context
                              ?? default;

        // Inject the current activity context into the message headers
        Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), payloadDescriptor, InjectTraceContextToPayload);

        return activity;
    }

    private static void InjectTraceContextToPayload(PayloadDescriptor descriptor, string key, string value)
    {
        descriptor.AddHeader(key, value);
    }

    private static IEnumerable<string> ExtractFromMetadata(Metadata metadata, string key)
    {
        yield return metadata[key];
    }
}