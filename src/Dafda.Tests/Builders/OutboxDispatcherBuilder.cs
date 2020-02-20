using Dafda.Outbox;
using Dafda.Producing;

namespace Dafda.Tests.Builders
{
    internal class OutboxDispatcherBuilder
    {
        private IOutboxUnitOfWorkFactory _outboxUnitOfWorkFactory;
        private IProducer _producer;

        public OutboxDispatcherBuilder With(IOutboxUnitOfWorkFactory outboxUnitOfWorkFactory)
        {
            _outboxUnitOfWorkFactory = outboxUnitOfWorkFactory;
            return this;
        }

        public OutboxDispatcherBuilder With(IProducer producer)
        {
            _producer = producer;
            return this;
        }

        public OutboxDispatcher Build()
        {
            return new OutboxDispatcher(_outboxUnitOfWorkFactory, _producer);
        }

        public static implicit operator OutboxDispatcher(OutboxDispatcherBuilder builder)
        {
            return builder.Build();
        }
    }
}