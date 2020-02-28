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

            var outboxProducerOptions = new OutboxOptions(services, outgoingMessageRegistry);
            options?.Invoke(outboxProducerOptions);
            var producerConfiguration = outboxProducerOptions.Build();

            var outboxWaiter = new OutboxNotification(outboxProducerOptions.DispatchInterval);

            services.AddTransient<OutboxQueue>(provider =>
            {
                var messageIdGenerator = producerConfiguration.MessageIdGenerator;
                var outboxMessageRepository = provider.GetRequiredService<IOutboxMessageRepository>();
                return new OutboxQueue(messageIdGenerator, outgoingMessageRegistry, outboxMessageRepository, outboxWaiter);
            });
        }

        public static void AddOutboxProducer(this IServiceCollection services, Action<OutboxProducerOptions> options)
        {
            var builder = new ProducerConfigurationBuilder();
            var outboxProducerOptions = new OutboxProducerOptions(builder, services);
            options?.Invoke(outboxProducerOptions);
            var producerConfiguration = builder.Build();

            var outboxWaiter = new OutboxNotification(outboxProducerOptions.DispatchInterval);

            services.AddTransient<IHostedService, OutboxDispatcherHostedService>(provider =>
            {
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