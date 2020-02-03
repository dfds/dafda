using System;
using Dafda.Outbox;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dafda.Configuration
{
    public interface IOutboxOptions
    {
        void WithOutboxMessageRepository<T>() where T : class, IOutboxMessageRepository;
        void WithOutboxMessageRepository(Func<IServiceProvider,IOutboxMessageRepository> implementationFactory);
        void WithOutboxPublisher(Action<IOutboxPublisherOptions> config);
    }

    internal class OutboxOptions : IOutboxOptions
    {
        private readonly IServiceCollection _services;

        public OutboxOptions(IServiceCollection services)
        {
            _services = services;
        }

        public void WithOutboxMessageRepository<T>() where T : class, IOutboxMessageRepository
        {
            _services.AddTransient<IOutboxMessageRepository, T>();
        }

        public void WithOutboxMessageRepository(Func<IServiceProvider, IOutboxMessageRepository> implementationFactory)
        {
            _services.AddTransient(implementationFactory);
        }

        public void WithOutboxPublisher(Action<IOutboxPublisherOptions> config)
        {
            var configuration = new OutboxPublisherOptions(_services);
            config?.Invoke(configuration);

            // TODO -- outbox producer
            //_services.AddHostedService<PollingPublisher>();
            _services.AddTransient<IHostedService, PollingPublisher>(provider => new PollingPublisher(
                unitOfWorkFactory: provider.GetRequiredService<IOutboxUnitOfWorkFactory>(),
                producer: provider.GetRequiredService<IProducer>(),
                dispatchInterval: configuration.DispatchInterval
            ));
        }
    }

    public interface IOutboxPublisherOptions 
    {
        void WithUnitOfWorkFactory<T>() where T : class, IOutboxUnitOfWorkFactory;
        void WithUnitOfWorkFactory(Func<IServiceProvider, IOutboxUnitOfWorkFactory> implementationFactory);
        void WithDispatchInterval(TimeSpan interval);
    }

    internal class OutboxPublisherOptions : IOutboxPublisherOptions
    {
        private readonly IServiceCollection _services;

        public OutboxPublisherOptions(IServiceCollection services)
        {
            _services = services;
        }

        public void WithUnitOfWorkFactory<T>() where T : class, IOutboxUnitOfWorkFactory
        {
            _services.AddTransient<IOutboxUnitOfWorkFactory, T>();
        }

        public void WithUnitOfWorkFactory(Func<IServiceProvider, IOutboxUnitOfWorkFactory> implementationFactory)
        {
            _services.AddTransient(implementationFactory);
        }

        public TimeSpan DispatchInterval { get; private set; } = TimeSpan.FromSeconds(5);

        public void WithDispatchInterval(TimeSpan interval)
        {
            DispatchInterval = interval;
        }
    }
}