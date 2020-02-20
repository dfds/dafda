using System;
using Dafda.Outbox;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dafda.Configuration
{
    public static class OutboxProducerServiceCollectionExtensions
    {
        public static void AddOutbox(this IServiceCollection services, Action<OutboxProducerOptions> options)
        {
            var outgoingMessageRegistry1 = new OutgoingMessageRegistry();
            var configurationBuilder = new ProducerConfigurationBuilder();
            configurationBuilder.WithOutgoingMessageRegistry(outgoingMessageRegistry1);
            var outboxProducerOptions = new OutboxProducerOptions(configurationBuilder, services, outgoingMessageRegistry1);
            options?.Invoke(outboxProducerOptions);
            var producerConfiguration = configurationBuilder.Build();

            services.AddSingleton<IOutboxWaiter, OutboxWaiter>(provider => new OutboxWaiter(outboxProducerOptions.DispatchInterval));

            services.AddTransient<OutboxQueue>(provider =>
            {
                var outgoingMessageRegistry = producerConfiguration.OutgoingMessageRegistry;
                var messageIdGenerator = producerConfiguration.MessageIdGenerator;
                var outboxMessageRepository = provider.GetRequiredService<IOutboxMessageRepository>();
                return new OutboxQueue(messageIdGenerator, outgoingMessageRegistry, outboxMessageRepository);
            });

            services.AddTransient<IHostedService, OutboxDispatcherHostedService>(provider =>
            {
                var kafkaProducerFactory = producerConfiguration.KafkaProducerFactory;
                var kafkaProducer = kafkaProducerFactory.CreateProducer();
                var outgoingMessageRegistry = producerConfiguration.OutgoingMessageRegistry;
                var messageIdGenerator = producerConfiguration.MessageIdGenerator;

                var producer = new Producer(kafkaProducer, outgoingMessageRegistry, messageIdGenerator);

                return new OutboxDispatcherHostedService(
                    unitOfWorkFactory: provider.GetRequiredService<IOutboxUnitOfWorkFactory>(),
                    producer: producer,
                    outboxWaiter: provider.GetRequiredService<IOutboxWaiter>()
                );
            });
        }
    }
}