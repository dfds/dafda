using System.Diagnostics;
using System.Reflection;
using Dafda.Producing;
using Dafda.Serializing;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Dafda.Diagnostics;

internal static class ProducerActivitySource
{
    private const string ActivityNameSuffix = "send";
    private static readonly AssemblyName AssemblyName = typeof(KafkaProducer).Assembly.GetName();
    private static readonly ActivitySource ActivitySource = new(AssemblyName.Name, AssemblyName.Version.ToString());
    public static TextMapPropagator Propagator { get; set; } = Propagators.DefaultTextMapPropagator;

    public static Activity StartActivity(PayloadDescriptor payloadDescriptor)
    {
        // Start the activity
        var activity = ActivitySource.StartActivity($"{payloadDescriptor.TopicName} {payloadDescriptor.MessageType} {ActivityNameSuffix}", ActivityKind.Producer)
            .AddDefaultOpenTelemetryTags(
                destinationName: payloadDescriptor.TopicName,
                messageId: payloadDescriptor.MessageId,
                clientId: payloadDescriptor.ClientId,
                partitionKey: payloadDescriptor.PartitionKey)
            .AddProducerOpenTelemetryTags();

        // Extract the current activity context
        var contextToInject = activity?.Context
                              ?? default;

        // Inject the current activity context into the message headers
        Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), payloadDescriptor, InjectTraceContext);

        return activity;
    }

    private static void InjectTraceContext(PayloadDescriptor descriptor, string key, string value)
    {
        descriptor.AddHeader(key, value);
    }
}