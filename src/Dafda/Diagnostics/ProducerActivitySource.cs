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
        // Extract the current activity context
        var contextToInject = Activity.Current?.Context
                              ?? default;

        // Inject the current context into the message headers
        Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), payloadDescriptor, InjectTraceContext);

        // Start the activity
        return ActivitySource.StartActivity($"{payloadDescriptor.TopicName} {ActivityNameSuffix}", ActivityKind.Producer)
            .AddDefaultOpenTelemetryTags(
                destinationName: payloadDescriptor.TopicName,
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