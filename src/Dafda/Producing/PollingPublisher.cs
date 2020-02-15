using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Outbox;
using Microsoft.Extensions.Hosting;

namespace Dafda.Producing
{
    internal class PollingPublisher : IHostedService, IDisposable
    {
        private readonly IOutboxWaiter _outboxWaiter;
        private readonly OutboxProcessor _outboxProcessor;
        private Thread _thread;
        private CancellationTokenSource _cancellationTokenSource;

        public PollingPublisher(IOutboxUnitOfWorkFactory unitOfWorkFactory, IProducer producer, IOutboxWaiter outboxWaiter)
        {
            _outboxWaiter = outboxWaiter;
            _outboxProcessor = new OutboxProcessor(unitOfWorkFactory, producer);
        }

        public Task ProcessUnpublishedOutboxMessages(CancellationToken stoppingToken)
        {
            return _outboxProcessor.ProcessUnpublishedOutboxMessages(stoppingToken);
        }

        private void ThreadProc()
        {
            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    ProcessUnpublishedOutboxMessages(_cancellationTokenSource.Token).Wait();
                    _outboxWaiter.Wait();
                }
            }
            catch (ThreadAbortException)
            {
                
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
            
            _outboxWaiter.WakeUp();
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