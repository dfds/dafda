using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Outbox;
using Microsoft.Extensions.Hosting;

namespace Dafda.Producing
{
    internal class OutboxDispatcherHostedService : IHostedService, IDisposable
    {
        private readonly IOutboxListener _outboxListener;
        private readonly OutboxDispatcher _outboxDispatcher;
        
        private CancellationTokenSource _cancellationTokenSource;
        private Thread _thread;

        public OutboxDispatcherHostedService(IOutboxUnitOfWorkFactory unitOfWorkFactory, OutboxProducer producer, IOutboxListener outboxListener)
        {
            _outboxListener = outboxListener;
            _outboxDispatcher = new OutboxDispatcher(unitOfWorkFactory, producer);
        }

        private void ThreadProc()
        {
            try
            {
                ProcessOutbox(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ThreadAbortException)
            {
            }
        }

        public void ProcessOutbox(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _outboxDispatcher.Dispatch(cancellationToken).Wait(cancellationToken);
                _outboxListener.Wait(cancellationToken);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            _thread = new Thread(ThreadProc);
            _thread.IsBackground = true;

            _thread.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Stop();
            return Task.CompletedTask;
        }

        private void Stop()
        {
            _cancellationTokenSource?.Cancel();

            _thread?.Join();
            _thread = null;
        }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
        }
    }
}