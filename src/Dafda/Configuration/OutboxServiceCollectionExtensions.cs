using System;
using Dafda.Outbox;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            services.AddTransient(provider =>
            {
                var messageIdGenerator = configuration.MessageIdGenerator;
                var outboxMessageRepository = provider.GetRequiredService<IOutboxMessageRepository>();
                var outboxNotifier = configuration.Notifier;

                return new OutboxQueue(
                    messageIdGenerator,
                    outgoingMessageRegistry,
                    outboxMessageRepository,
                    outboxNotifier
                );
            });
        }

        public static void AddOutboxProducer(this IServiceCollection services, Action<OutboxProducerOptions> options)
        {
            var builder = new ProducerConfigurationBuilder();
            var outboxProducerOptions = new OutboxProducerOptions(builder, services);
            options?.Invoke(outboxProducerOptions);
            var configuration = builder.Build();

            var outboxListener = outboxProducerOptions.OutboxListener;

            services.AddTransient<IHostedService, OutboxDispatcherHostedService>(provider =>
            {
                var outboxUnitOfWorkFactory = provider.GetRequiredService<IOutboxUnitOfWorkFactory>();
                var kafkaProducer = configuration.KafkaProducerFactory();
                var producer = new OutboxProducer(kafkaProducer);

                return new OutboxDispatcherHostedService(
                    outboxUnitOfWorkFactory,
                    producer,
                    outboxListener
                );
            });
        }
    }
}