using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Diagnostics;
using Dafda.Consuming.Interfaces;
using Dafda.Consuming.MessageFilters;
using Microsoft.Extensions.Logging;

namespace Dafda.Consuming
{
    internal class Consumer : IConsumer
    {
        private readonly ILogger<Consumer> _logger;
        private readonly IConsumerScopeFactory _consumerScopeFactory;
        private readonly bool _isAutoCommitEnabled;
        private readonly LocalMessageDispatcher _localMessageDispatcher;
        private readonly MessageFilter _messageFilter;

        public Consumer(
            ILogger<Consumer> logger,
            MessageHandlerRegistry messageHandlerRegistry,
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
            _logger = logger;
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

            using var scope = _logger.BeginScope("{TraceParent}", messageResult.Message.Metadata["traceparent"]);

            var message = messageResult.Message;
            using var activity = ConsumerActivitySource.StartActivity(messageResult);
            _logger.LogDebug("Starting new activity Consumer:{ParentActivityId}:{ActivityId}", activity?.ParentId,
                activity?.Id);

            if (_messageFilter.CanAcceptMessage(messageResult))
                await _localMessageDispatcher.Dispatch(message);

            if (!_isAutoCommitEnabled)
                await messageResult.Commit();
        }
    }
}