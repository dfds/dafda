using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Consuming
{
    internal class Consumer
    {
        private readonly LocalMessageDispatcher _localMessageDispatcher;
        private readonly IConsumerScopeFactory _consumerScopeFactory;
        private readonly bool _isAutoCommitEnabled;

        public Consumer(
            MessageHandlerRegistry messageHandlerRegistry,
            IHandlerUnitOfWorkFactory unitOfWorkFactory,
            IConsumerScopeFactory consumerScopeFactory,
            IUnconfiguredMessageHandlingStrategy fallbackHandler,
            bool isAutoCommitEnabled = false)
        {
            _localMessageDispatcher =
                new LocalMessageDispatcher(
                    messageHandlerRegistry,
                    unitOfWorkFactory,
                    fallbackHandler);
            _consumerScopeFactory =
                consumerScopeFactory
                ?? throw new ArgumentNullException(nameof(consumerScopeFactory));
            _isAutoCommitEnabled = isAutoCommitEnabled;
        }

        public async Task ConsumeAll(CancellationToken cancellationToken)
        {
            using (var consumerScope = _consumerScopeFactory.CreateConsumerScope())
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await ProcessNextMessage(consumerScope, cancellationToken);
                }
            }
        }

        public async Task ConsumeSingle(CancellationToken cancellationToken)
        {
            using (var consumerScope = _consumerScopeFactory.CreateConsumerScope())
            {
                await ProcessNextMessage(consumerScope, cancellationToken);
            }
        }

        private async Task ProcessNextMessage(ConsumerScope consumerScope, CancellationToken cancellationToken)
        {
            var messageResult = await consumerScope.GetNext(cancellationToken);
            await _localMessageDispatcher.Dispatch(messageResult.Message);

            if (!_isAutoCommitEnabled)
            {
                await messageResult.Commit();
            }
        }
    }
}
