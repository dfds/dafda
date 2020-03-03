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
            using (var outboxUnitOfWork = _unitOfWorkFactory.Begin())
            {
                var outboxMessages = await outboxUnitOfWork.GetAllUnpublishedMessages(cancellationToken);

                _logger.LogDebug("Unpublished outbox messages: {OutboxMessageCount}", outboxMessages.Count);

                try
                {
                    foreach (var outboxMessage in outboxMessages)
                    {
                        await _producer.Produce(outboxMessage);

                        outboxMessage.MaskAsProcessed();

                        _logger.LogDebug(@"Published outbox message {MessageId} ({Type})", outboxMessage.MessageId, outboxMessage.Type);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogDebug("Error while publishing outbox messages", exception);
                    throw;
                }

                await outboxUnitOfWork.Commit(cancellationToken);
            }
        }
    }
}