using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Diagnostics;
using Dafda.Consuming.Interfaces;
using Dafda.Consuming.MessageFilters;

namespace Dafda.Consuming
{
    internal class Consumer : IConsumer
    {
        private readonly IConsumerScopeFactory _consumerScopeFactory;
        private readonly bool _isAutoCommitEnabled;
        private readonly LocalMessageDispatcher _localMessageDispatcher;
        private readonly MessageFilter _messageFilter;

        public Consumer(MessageHandlerRegistry messageHandlerRegistry,
            IHandlerUnitOfWorkFactory unitOfWorkFactory,
            IConsumerScopeFactory consumerScopeFactory,
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
                    await ProcessNextMessage(consumerScope, cancellationToken);
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
            using var activity = ConsumerActivitySource.StartActivity(messageResult);

            var message = messageResult.Message;
            if (_messageFilter.CanAcceptMessage(messageResult))
                await _localMessageDispatcher.Dispatch(message);

            if (!_isAutoCommitEnabled)
                await messageResult.Commit();
        }
    }
}