using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Outbox;
using Microsoft.Extensions.Hosting;

namespace Dafda.Producing
{
    internal class OutboxDispatcherHostedService : BackgroundService, IDisposable
    {
        private readonly IOutboxListener _outboxListener;
        private readonly OutboxDispatcher _outboxDispatcher;

        public OutboxDispatcherHostedService(IOutboxListener outboxListener, OutboxDispatcher outboxDispatcher)
        {
            _outboxListener = outboxListener;
            _outboxDispatcher = outboxDispatcher;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _outboxDispatcher.Dispatch(cancellationToken);
                await _outboxListener.Wait(cancellationToken);
            }
        }
    }
}
