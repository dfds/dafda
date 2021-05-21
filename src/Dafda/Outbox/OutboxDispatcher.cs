using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Producing;
using Microsoft.Extensions.Logging;

namespace Dafda.Outbox
{
    internal class OutboxDispatcher
    {
        private readonly ILogger<OutboxDispatcher> _logger;
        private readonly IOutboxUnitOfWorkFactory _unitOfWorkFactory;
        private readonly OutboxProducer _producer;

        public OutboxDispatcher(ILoggerFactory loggerFactory, IOutboxUnitOfWorkFactory unitOfWorkFactory, OutboxProducer producer)
        {
            _logger = loggerFactory.CreateLogger<OutboxDispatcher>();
            _unitOfWorkFactory = unitOfWorkFactory;
            _producer = producer;
        }

        public async Task Dispatch(CancellationToken cancellationToken)
        {
            using (var outboxUnitOfWork = await _unitOfWorkFactory.Begin(cancellationToken))
            {
                var entries = await outboxUnitOfWork.GetAllUnpublishedEntries(cancellationToken);

                _logger.LogDebug("Unpublished outbox entries: {EntryCount}", entries.Count);

                try
                {
                    foreach (var entry in entries)
                    {
                        await _producer.Produce(entry);

                        entry.MaskAsProcessed();

                        _logger.LogDebug(@"Published outbox message {MessageId}", entry.MessageId);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError("Error while publishing outbox messages", exception);
                    throw;
                }

                await outboxUnitOfWork.Commit(cancellationToken);
            }
        }
    }
}
