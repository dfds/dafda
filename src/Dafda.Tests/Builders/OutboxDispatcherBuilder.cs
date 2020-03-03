using Dafda.Outbox;
using Dafda.Producing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dafda.Tests.Builders
{
    internal class OutboxDispatcherBuilder
    {
        private IOutboxUnitOfWorkFactory _outboxUnitOfWorkFactory;
        private OutboxProducer _producer;

        public OutboxDispatcherBuilder With(IOutboxUnitOfWorkFactory outboxUnitOfWorkFactory)
        {
            _outboxUnitOfWorkFactory = outboxUnitOfWorkFactory;
            return this;
        }

        public OutboxDispatcherBuilder With(OutboxProducer producer)
        {
            _producer = producer;
            return this;
        }

        public OutboxDispatcher Build()
        {
            return new OutboxDispatcher(NullLoggerFactory.Instance, _outboxUnitOfWorkFactory, _producer);
        }

        public static implicit operator OutboxDispatcher(OutboxDispatcherBuilder builder)
        {
            return builder.Build();
        }
    }
}