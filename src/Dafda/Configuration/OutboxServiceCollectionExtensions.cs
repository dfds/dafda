using System;
using Dafda.Outbox;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
    public static class OutboxServiceCollectionExtensions
    {
        public static void AddOutbox(this IServiceCollection services, Action<OutboxOptions> options)
        {
            var outgoingMessageRegistry = new OutgoingMessageRegistry();

            var outboxOptions = new OutboxOptions(services, outgoingMessageRegistry);
            options?.Invoke(outboxOptions);
            var configuration = outboxOptions.Build();

            services.AddTransient(provider => new OutboxQueue(
                configuration.MessageIdGenerator,
                outgoingMessageRegistry,
                provider.GetRequiredService<IOutboxEntryRepository>(),
                configuration.Notifier,
                configuration.TopicPayloadSerializerRegistry
            ));
        }

        public static void AddOutboxProducer(this IServiceCollection services, Action<OutboxProducerOptions> options)
        {
            var builder = new ProducerConfigurationBuilder();
            var outboxProducerOptions = new OutboxProducerOptions(builder, services);
            options?.Invoke(outboxProducerOptions);
            var configuration = builder.Build();

            var outboxListener = outboxProducerOptions.OutboxListener;
            if (outboxListener == null)
            {
                throw new InvalidConfigurationException($"No {nameof(IOutboxListener)} was registered. Please use the {nameof(OutboxProducerOptions.WithListener)} in the {nameof(AddOutboxProducer)} configuration.");
            }

            services.AddTransient<IHostedService, OutboxDispatcherHostedService>(provider =>
            {
                var outboxUnitOfWorkFactory = provider.GetRequiredService<IOutboxUnitOfWorkFactory>();
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var kafkaProducer = configuration.KafkaProducerFactory(loggerFactory);
                var producer = new OutboxProducer(kafkaProducer);
                var outboxDispatcher = new OutboxDispatcher(loggerFactory, outboxUnitOfWorkFactory, producer);

                return new OutboxDispatcherHostedService(outboxListener, outboxDispatcher);
            });
        }
    }
}