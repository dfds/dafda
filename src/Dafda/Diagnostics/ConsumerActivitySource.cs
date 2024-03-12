using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Dafda.Consuming;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Dafda.Diagnostics;

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