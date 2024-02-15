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

            activity?.SetTag(OpenTelemetryMessagingAttributes.SYSTEM, "kafka");
            activity?.SetTag(OpenTelemetryMessagingAttributes.DESTINATION, messageResult.Topic);
            activity?.SetTag(OpenTelemetryMessagingAttributes.DESTINATION_KIND, "topic");
            activity?.SetTag(OpenTelemetryMessagingAttributes.MESSAGE_ID, message.Metadata.MessageId);
            activity?.SetTag(OpenTelemetryMessagingAttributes.CONVERSATION_ID, message.Metadata.CorrelationId);
            activity?.SetTag(OpenTelemetryMessagingAttributes.CLIENT_ID, messageResult.ClientId);

            // kafka
            activity?.SetTag(OpenTelemetryMessagingAttributes.KAFKA_MESSAGE_KEY, messageResult.PartitionKey);
            activity?.SetTag(OpenTelemetryMessagingAttributes.KAFKA_PARTITION, messageResult.Partition);

            // consumer
            activity?.SetTag(OpenTelemetryMessagingAttributes.KAFKA_CONSUMER_GROUP, messageResult.GroupId);
            activity?.SetTag(OpenTelemetryMessagingAttributes.OPERATION, "receive");

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

    /// <summary>
    /// Provides the OpenTelemetry messaging attributes.
    /// The complete list of messaging attributes specification is available here: https://opentelemetry.io/docs/specs/semconv/messaging/messaging-spans/#messaging-attributes
    /// </summary>
    internal static class OpenTelemetryMessagingAttributes
    {
        /// <summary>
        /// Message system. For Kafka, attribute value must be "kafka".
        /// </summary>
        public const string SYSTEM = "messaging.system";

        /// <summary>
        /// Message destination. For Kafka, attribute value must be a Kafka topic.
        /// </summary>
        public const string DESTINATION = "messaging.destination";

        /// <summary>
        /// Destination kind. For Kafka, attribute value must be "topic".
        /// </summary>
        public const string DESTINATION_KIND = "messaging.destination_kind";

        /// <summary>
        /// A value used by the messaging system as an identifier for the message, represented as a string.
        /// </summary>
        public const string MESSAGE_ID = "messaging.message.id";

        /// <summary>
        /// A string identifying the kind of messaging operation
        /// </summary>
        public const string OPERATION = "messaging.operation";

        /// <summary>
        /// The conversation ID identifying the conversation to which the message belongs, represented as a string. Sometimes called “Correlation ID”.
        /// </summary>
        public const string CONVERSATION_ID = "messaging.message.conversation_id";

        /// <summary>
        /// A unique identifier for the client that consumes or produces a message.
        /// </summary>
        public const string CLIENT_ID = "messaging.client_id";

        /// <summary>
        /// Kafka partition number.
        /// </summary>
        public const string KAFKA_PARTITION = "messaging.kafka.destination.partition";

        /// <summary>
        /// Kafka message key.
        /// </summary>
        public const string KAFKA_MESSAGE_KEY = "messaging.kafka.message_key";

        /// <summary>
        /// Name of the Kafka Consumer Group that is handling the message. Only applies to consumers, not producers.
        /// </summary>
        public const string KAFKA_CONSUMER_GROUP = "messaging.kafka.consumer.group";
    }
}