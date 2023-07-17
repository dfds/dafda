using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Consuming.Interfaces;
using Dafda.Consuming.MessageFilters;

namespace Dafda.Consuming
{
    internal class Consumer : IConsumer
    {
        private readonly LocalMessageDispatcher _localMessageDispatcher;
        private readonly IConsumerScopeFactory<MessageResult> _consumerScopeFactory;
        private readonly MessageFilter _messageFilter;
        private readonly bool _isAutoCommitEnabled;

        public Consumer(
            MessageHandlerRegistry messageHandlerRegistry,
            IHandlerUnitOfWorkFactory unitOfWorkFactory,
            IConsumerScopeFactory<MessageResult> consumerScopeFactory,
            IUnconfiguredMessageHandlingStrategy fallbackHandler,
            MessageFilter messageFilter,
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
            _messageFilter = messageFilter;
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

        private async Task ProcessNextMessage(IConsumerScope<MessageResult> consumerScope, CancellationToken cancellationToken)
        {
            var messageResult = await consumerScope.GetNext(cancellationToken);

            if(_messageFilter.CanAcceptMessage(messageResult))
            {
                await _localMessageDispatcher.Dispatch(messageResult.Message);
            }

            if (!_isAutoCommitEnabled)
            {
                await messageResult.Commit();
            }
        }
    }
}
