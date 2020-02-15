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
                var outboxMessages = await outboxUnitOfWork.GetAllUnpublishedMessages(cancellationToken);

                Log.Debug("Unpublished outbox messages: {OutboxMessageCount}", outboxMessages.Count);

                try
                {
                    foreach (var outboxMessage in outboxMessages)
                    {
                        await _producer.Produce(outboxMessage);

                        outboxMessage.MaskAsProcessed();

                        Log.Debug(@"Published outbox message {MessageId} ({Type})", outboxMessage.MessageId, outboxMessage.Type);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error("Error while publishing outbox messages", exception);
                    throw;
                }

                await outboxUnitOfWork.Commit(cancellationToken);
            }
        }
    }
    
    public interface IOutboxWaiter
    {
        void WakeUp();
        void Wait();
    }
    
    public class OutboxWaiter : IOutboxWaiter, IDisposable
    {
        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private readonly TimeSpan _delay;

        public OutboxWaiter(TimeSpan delay)
        {
            _delay = delay;
        }
        
        public void WakeUp()
        {
            _autoResetEvent.Set();
        }

        public void Wait()
        {
            _autoResetEvent.WaitOne(_delay);
        }

        public void Dispose()
        {
            _autoResetEvent.Dispose();
        }
    }
}