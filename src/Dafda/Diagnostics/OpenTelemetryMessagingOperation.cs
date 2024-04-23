namespace Dafda.Diagnostics;

internal static class OpenTelemetryMessagingOperation
{
    internal static class Producer
    {
        /// <summary>
        /// A message is created or passed to a client library for publishing. “Create” spans always refer to a single message and are used to provide a unique creation context for messages in batch publishing scenarios.
        /// </summary>
        public const string Create = "create";

        /// <summary>
        /// One or more messages are provided for publishing to an intermediary. If a single message is published, the context of the “Publish” span can be used as the creation context and no “Create” span needs to be created.
        /// </summary>
        public const string Publish = "publish";
    }

    internal static class Consumer
    {
        /// <summary>
        /// One or more messages are requested by a consumer. This operation refers to pull-based scenarios, where consumers explicitly call methods of messaging SDKs to receive messages.
        /// </summary>
        public const string Receive = "receive";
    }
}