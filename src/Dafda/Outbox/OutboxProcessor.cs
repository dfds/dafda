using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Logging;
using Dafda.Producing;

namespace Dafda.Outbox
{
    public class OutboxProcessor
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        private readonly IOutboxUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IProducer _producer;

        public OutboxProcessor(IOutboxUnitOfWorkFactory unitOfWorkFactory, IProducer producer)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _producer = producer;
        }

        public async Task ProcessUnpublishedOutboxMessages(CancellationToken cancellationToken)
        {
            using (var outboxUnitOfWork = _unitOfWorkFactory.Begin())
            {
                var domainEvents = await outboxUnitOfWork.GetAllUnpublishedMessages(cancellationToken);

                Log.Debug("Domain events to publish: {DomainEventsCount}", domainEvents.Count);

                try
                {
                    foreach (OutboxMessage domainEvent in domainEvents)
                    {
                        await _producer.Produce(domainEvent);

                        domainEvent.MaskAsProcessed();

                        Log.Debug(@"Domain event ""{Type}>{MessageId}"" has been published!", domainEvent.Type, domainEvent.MessageId);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error("Error while publishing domain events", exception);
                    throw;
                }

                await outboxUnitOfWork.Commit(cancellationToken);
            }
        }
    }
}