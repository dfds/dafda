using System.Diagnostics;

namespace Dafda.Diagnostics;

internal static class OpenTelemetryActivityExtensions
{
    private static class Messaging
    {
        public const string System = "kafka";
    }

    private static class Operation
    {
        public const string Receive = "receive";
        public const string Publish = "publish";
    }

    public static Activity AddDefaultMessagingTags(this Activity activity,
        string destinationName,
        string messageId,
        string clientId,
        string partitionKey,
        int? partition = null)
    {
        // handle null activity
        if (activity == null)
            return null;

        // messaging tags
        activity.SetTag(OpenTelemetryMessagingAttributes.System, Messaging.System);
        activity.SetTag(OpenTelemetryMessagingAttributes.DestinationName, destinationName);
        activity.SetTag(OpenTelemetryMessagingAttributes.MessageId, messageId);
        activity.SetTag(OpenTelemetryMessagingAttributes.ClientId, clientId);

        // kafka specific tags
        activity.SetTag(OpenTelemetryMessagingAttributes.Kafka.MessageKey, partitionKey);
        if (partition.HasValue)
            activity.SetTag(OpenTelemetryMessagingAttributes.Kafka.Partition, partition);

        return activity;
    }

    public static Activity AddConsumerMessagingTags(this Activity activity, string groupId)
    {
        if (activity == null)
            return null;

        activity.SetTag(OpenTelemetryMessagingAttributes.Kafka.ConsumerGroup, groupId);
        activity.SetTag(OpenTelemetryMessagingAttributes.Operation, Operation.Receive);
        return activity;
    }

    public static Activity AddProducerMessagingTags(this Activity activity)
    {
        if (activity == null)
            return null;

        activity.SetTag(OpenTelemetryMessagingAttributes.Operation, Operation.Publish);
        return activity;
    }
}