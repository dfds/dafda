using System;
using Dafda.Outbox;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dafda.Configuration
{
    public static class OutboxServiceCollectionExtensions
    {
        public static void AddOutbox(this IServiceCollection services, Action<OutboxProducerOptions> options)
        {
            var outgoingMessageRegistry = new OutgoingMessageRegistry();
            var configurationBuilder = new ProducerConfigurationBuilder();
            var outboxProducerOptions = new OutboxProducerOptions(configurationBuilder, services, outgoingMessageRegistry);
            options?.Invoke(outboxProducerOptions);
            var producerConfiguration = configurationBuilder.Build();

            var outboxWaiter = new OutboxNotification(outboxProducerOptions.DispatchInterval);

            services.AddTransient<OutboxQueue>(provider =>
            {
                var messageIdGenerator = producerConfiguration.MessageIdGenerator;
                var outboxMessageRepository = provider.GetRequiredService<IOutboxMessageRepository>();
                return new OutboxQueue(messageIdGenerator, outgoingMessageRegistry, outboxMessageRepository, outboxWaiter);
            });

            services.AddTransient<IHostedService, OutboxDispatcherHostedService>(provider =>
            {
                var messageIdGenerator = producerConfiguration.MessageIdGenerator;
                var kafkaProducer = producerConfiguration.KafkaProducerFactory();

                var producer = new OutboxProducer(kafkaProducer);

                return new OutboxDispatcherHostedService(
                    unitOfWorkFactory: provider.GetRequiredService<IOutboxUnitOfWorkFactory>(),
                    producer: producer,
                    outboxNotification: outboxWaiter
                );
            });
        }
    }
}