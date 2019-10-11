using Dafda.Outbox;
using Dafda.Producing;

namespace Dafda.Tests.Builders
{
    internal class OutboxProcessorBuilder
    {
        private IOutboxUnitOfWorkFactory _outboxUnitOfWorkFactory;
        private IProducer _producer;

        public OutboxProcessorBuilder With(IOutboxUnitOfWorkFactory outboxUnitOfWorkFactory)
        {
            _outboxUnitOfWorkFactory = outboxUnitOfWorkFactory;
            return this;
        }

        public OutboxProcessorBuilder With(IProducer producer)
        {
            _producer = producer;
            return this;
        }

        public OutboxProcessor Build()
        {
            return new OutboxProcessor(_outboxUnitOfWorkFactory, _producer);
        }

        public static implicit operator OutboxProcessor(OutboxProcessorBuilder builder)
        {
            return builder.Build();
        }
    }
}