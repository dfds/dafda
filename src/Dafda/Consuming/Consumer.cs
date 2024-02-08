using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Consuming.Interfaces;
using Dafda.Consuming.MessageFilters;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

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
            var parentContext = Propagator.Extract(default, message.Metadata, ExtractFromMetadata);
            Baggage.Current = parentContext.Baggage;

            _logger.LogDebug("Extracted ActivityContext: {@ActivityContext}", parentContext.ActivityContext);

            using var activity = DafdaActivitySource.ActivitySource.StartActivity($"{messageResult.Topic} receive", ActivityKind.Consumer, parentContext.ActivityContext);

            _logger.LogDebug("Starting new activity Consumer:{ParentActivityId}:{ActivityId}", activity?.ParentId, activity?.Id);

            activity?.SetTag("messaging.system", "kafka");
            activity?.SetTag("messaging.destination", messageResult.Topic);
            activity?.SetTag("messaging.destination_kind", "topic");
            activity?.SetTag("messaging.message_id", message.Metadata.MessageId);
            activity?.SetTag("messaging.conversation_id", message.Metadata.CorrelationId);
            //activity?.SetTag("messaging.message_payload_size_bytes", "0");
            // consumer
            activity?.SetTag("messaging.operation", "receive");
            activity?.SetTag("messaging.consumer_id", $"{messageResult.GroupId} - {messageResult.ClientId}");
            // kafka
            activity?.SetTag("messaging.kafka.message_key", messageResult.PartitionKey);
            activity?.SetTag("messaging.kafka.consumer_group", messageResult.GroupId);
            activity?.SetTag("messaging.kafka.client_id", messageResult.ClientId);
            activity?.SetTag("messaging.kafka.partition", messageResult.Partition);

            if (_messageFilter.CanAcceptMessage(messageResult))
                await _localMessageDispatcher.Dispatch(message);

            if (!_isAutoCommitEnabled) await messageResult.Commit();
        }

        public static TextMapPropagator Propagator { get; set; } = Propagators.DefaultTextMapPropagator;

        private static IEnumerable<string> ExtractFromMetadata(Metadata metadata, string key)
        {
            yield return metadata[key];
        }

    }

    internal static class DafdaActivitySource
    {
        public static readonly AssemblyName AssemblyName = typeof(KafkaConsumerScope).Assembly.GetName();
        public static readonly ActivitySource ActivitySource = new(AssemblyName.Name, AssemblyName.Version.ToString());
    }
}