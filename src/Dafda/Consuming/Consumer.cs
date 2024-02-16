using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming.Interfaces;
using Dafda.Consuming.MessageFilters;
using Dafda.Serializing;
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

            using var activity = DafdaActivitySource.Consumer.StartActivity(messageResult, parentContext.ActivityContext);
            _logger.LogDebug("Starting new activity Consumer:{ParentActivityId}:{ActivityId}", activity?.ParentId, activity?.Id);

            if (_messageFilter.CanAcceptMessage(messageResult))
                await _localMessageDispatcher.Dispatch(message);

            if (!_isAutoCommitEnabled)
                await messageResult.Commit();
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

        public static readonly IActivityBuilder<MessageResult> Consumer = new ConsumerActivityBuilder();

        private const string MessagingSystem = "kafka";
        private const string DestinationKind = "topic";
        private const string OperationReceive = "receive";
        private const string OperationPublish = "publish";

        internal interface IActivityBuilder<in T>
        {
            Activity StartActivity(T data, ActivityContext parentContext);
        }

        internal class ConsumerActivityBuilder : IActivityBuilder<MessageResult>
        {
            private const string ConsumerActivityNameSuffix = "receive";

            public Activity StartActivity(MessageResult data, ActivityContext parentContext)
            {
                return ActivitySource
                    .StartActivity($"{data.Topic} {ConsumerActivityNameSuffix}", ActivityKind.Consumer, parentContext)
                    .AddDefaultOpenTelemetryTags(
                        topicName: data.Topic,
                        messageId: data.Message.Metadata.MessageId,
                        conversationId : data.Message.Metadata.CorrelationId,
                        clientId: data.ClientId,
                        partitionKey: data.PartitionKey,
                        partition: data.Partition)
                    .AddConsumerOpenTelemetryTags(
                        groupId: data.GroupId);
            }

        }

        internal class ProducerActivityBuilder : IActivityBuilder<PayloadDescriptor>
        {
            private const string ProducerActivityNameSuffix = "send";

            public Activity StartActivity(PayloadDescriptor data, ActivityContext parentContext)
            {
                return ActivitySource
                    .StartActivity($"{data.TopicName} {ProducerActivityNameSuffix}", ActivityKind.Producer,
                        parentContext)
                    .AddDefaultOpenTelemetryTags(
                        topicName: data.TopicName,
                        messageId: data.MessageId,
                        conversationId: "",
                        clientId: data.ClientId,
                        partitionKey: data.PartitionKey,
                        partition: 0)
                    .AddProducerOpenTelemetryTags();
            }
        }

        private static Activity AddDefaultOpenTelemetryTags(this Activity activity,
            string topicName,
            string messageId,
            string conversationId,
            string clientId,
            string partitionKey,
            int partition)
        {
            // messaging tags
            activity.SetTag(OpenTelemetryMessagingAttributes.SYSTEM, MessagingSystem);
            activity.SetTag(OpenTelemetryMessagingAttributes.DESTINATION_KIND, DestinationKind);

            activity.SetTag(OpenTelemetryMessagingAttributes.DESTINATION, topicName);
            activity.SetTag(OpenTelemetryMessagingAttributes.MESSAGE_ID, messageId);
            activity.SetTag(OpenTelemetryMessagingAttributes.CONVERSATION_ID, conversationId);
            activity.SetTag(OpenTelemetryMessagingAttributes.CLIENT_ID, clientId);

            // kafka specific tags
            activity.SetTag(OpenTelemetryMessagingAttributes.KAFKA_MESSAGE_KEY, partitionKey);
            activity.SetTag(OpenTelemetryMessagingAttributes.KAFKA_PARTITION, partition);

            return activity;
        }


        public static Activity AddConsumerOpenTelemetryTags(this Activity activity, string groupId)
        {
            activity.SetTag(OpenTelemetryMessagingAttributes.KAFKA_CONSUMER_GROUP, groupId);
            activity.SetTag(OpenTelemetryMessagingAttributes.OPERATION, OperationReceive);
            return activity;
        }

        public static Activity AddProducerOpenTelemetryTags(this Activity activity)
        {
            activity.SetTag(OpenTelemetryMessagingAttributes.OPERATION, OperationPublish);
            return activity;
        }
    }
}