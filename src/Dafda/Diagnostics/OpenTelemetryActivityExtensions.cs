using System.Diagnostics;

namespace Dafda.Diagnostics;

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
        string clientId,
        string partitionKey,
        int? partition = null)
    {
        // handle null activity
        if (activity == null)
            return null;

        // messaging tags
        activity.SetTag(OpenTelemetryMessagingAttributes.SYSTEM, Messaging.System);
        activity.SetTag(OpenTelemetryMessagingAttributes.DESTINATION_KIND, Messaging.DestinationKind);

        activity.SetTag(OpenTelemetryMessagingAttributes.DESTINATION, topicName);
        activity.SetTag(OpenTelemetryMessagingAttributes.MESSAGE_ID, messageId);
        activity.SetTag(OpenTelemetryMessagingAttributes.CLIENT_ID, clientId);

        // kafka specific tags
        activity.SetTag(OpenTelemetryMessagingAttributes.KAFKA_MESSAGE_KEY, partitionKey);
        if (partition.HasValue)
            activity.SetTag(OpenTelemetryMessagingAttributes.KAFKA_PARTITION, partition);

        return activity;
    }

    public static Activity AddConsumerOpenTelemetryTags(this Activity activity, string groupId)
    {
        if (activity == null)
            return null;

        activity.SetTag(OpenTelemetryMessagingAttributes.KAFKA_CONSUMER_GROUP, groupId);
        activity.SetTag(OpenTelemetryMessagingAttributes.OPERATION, Operation.Receive);
        return activity;
    }

    public static Activity AddProducerOpenTelemetryTags(this Activity activity)
    {
        if (activity == null)
            return null;

        activity.SetTag(OpenTelemetryMessagingAttributes.OPERATION, Operation.Publish);
        return activity;
    }
}