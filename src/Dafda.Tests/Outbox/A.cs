using Dafda.Tests.Builders;

namespace Dafda.Tests.Outbox
{
    internal static class A
    {
        public static OutboxQueueBuilder OutboxQueue => new OutboxQueueBuilder();
        public static OutgoingMessageRegistryBuilder OutgoingMessageRegistry => new OutgoingMessageRegistryBuilder();
        public static ProducerBuilder Producer => new ProducerBuilder();
        public static OutboxProcessorBuilder OutboxProcessor => new OutboxProcessorBuilder();
        public static OutboxMessageBuilder OutboxMessage => new OutboxMessageBuilder();
    }
}