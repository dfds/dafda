using Dafda.Tests.Builders;

namespace Dafda.Tests.Outbox
{
    internal static class A
    {
        public static OutboxQueueBuilder OutboxQueue => new OutboxQueueBuilder();
        public static OutgoingMessageRegistryBuilder OutgoingMessageRegistry => new OutgoingMessageRegistryBuilder();
        public static OutboxProducerBuilder OutboxProducer => new OutboxProducerBuilder();
        public static OutboxDispatcherBuilder OutboxDispatcher => new OutboxDispatcherBuilder();
        public static OutboxMessageBuilder OutboxMessage => new OutboxMessageBuilder();
    }
}