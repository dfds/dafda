using System;
using Dafda.Outbox;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dafda.Configuration
{
    public static class ProducerServiceCollectionExtensions
    {
        public static void AddProducer(this IServiceCollection services, Action<ProducerOptions> options)
        {
            var configuration = ConfigureProducerConfiguration(services, options);

            services.AddSingleton<IProducer>(provider =>
            {
                var kafkaProducerFactory = configuration.KafkaProducerFactory;
                IKafkaProducer kafkaProducer = kafkaProducerFactory.CreateProducer();
                var outgoingMessageRegistry = configuration.OutgoingMessageRegistry;
                var messageIdGenerator = configuration.MessageIdGenerator;
                return new Producer(kafkaProducer, outgoingMessageRegistry, messageIdGenerator);
            });
        }

        private static ProducerConfiguration ConfigureProducerConfiguration(IServiceCollection services, Action<ProducerOptions> options)
        {
            var outgoingMessageRegistry = new OutgoingMessageRegistry();
            var configurationBuilder = new ProducerConfigurationBuilder();
            configurationBuilder.WithOutgoingMessageRegistry(outgoingMessageRegistry);
            var consumerOptions = new ProducerOptions(configurationBuilder, services, outgoingMessageRegistry);
            options?.Invoke(consumerOptions);
            var producerConfiguration = configurationBuilder.Build();
            return producerConfiguration;
        }

        public static void AddOutbox(this IServiceCollection services, Action<OutboxProducerOptions> options)
        {
            var outgoingMessageRegistry1 = new OutgoingMessageRegistry();
            var configurationBuilder = new ProducerConfigurationBuilder();
            configurationBuilder.WithOutgoingMessageRegistry(outgoingMessageRegistry1);
            var outboxProducerOptions = new OutboxProducerOptions(configurationBuilder, services, outgoingMessageRegistry1);
            options?.Invoke(outboxProducerOptions);
            var producerConfiguration = configurationBuilder.Build();

            services.AddSingleton<IOutboxWaiter, OutboxWaiter>(provider => new OutboxWaiter(outboxProducerOptions.DispatchInterval));

            services.AddTransient<IOutbox, OutboxMessageCollector>(provider =>
            {
                var outgoingMessageRegistry = producerConfiguration.OutgoingMessageRegistry;
                var messageIdGenerator = producerConfiguration.MessageIdGenerator;
                var outboxMessageRepository = provider.GetRequiredService<IOutboxMessageRepository>();
                return new OutboxMessageCollector(messageIdGenerator, outgoingMessageRegistry, outboxMessageRepository);
            });

            services.AddTransient<IHostedService, PollingPublisher>(provider =>
            {
                var kafkaProducerFactory = producerConfiguration.KafkaProducerFactory;
                var kafkaProducer = kafkaProducerFactory.CreateProducer();
                var outgoingMessageRegistry = producerConfiguration.OutgoingMessageRegistry;
                var messageIdGenerator = producerConfiguration.MessageIdGenerator;

                var producer = new Producer(kafkaProducer, outgoingMessageRegistry, messageIdGenerator);

                return new PollingPublisher(
                    unitOfWorkFactory: provider.GetRequiredService<IOutboxUnitOfWorkFactory>(),
                    producer: producer,
                    outboxWaiter: provider.GetRequiredService<IOutboxWaiter>()
                );
            });
        }
    }
}