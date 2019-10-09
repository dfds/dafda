using System;
using Dafda.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public interface IOutboxOptions
    {
        void WithDomainEventRepository<T>() where T : class, IOutboxMessageRepository;
        void WithDomainEventRepository(Func<IServiceProvider, IOutboxMessageRepository> implementationFactory);
        void RegisterDomainEvent<TDomainEvent>(string topicName, string eventTypeName, Func<TDomainEvent, string> keySelector) where TDomainEvent : class;
    }

    internal class OutboxOptions : IOutboxOptions
    {
        private readonly IServiceCollection _services;
        private readonly OutboxRegistry _registry;

        public OutboxOptions(IServiceCollection services, OutboxRegistry registry)
        {
            _services = services;
            _registry = registry;
        }

        public void WithDomainEventRepository<T>() where T : class, IOutboxMessageRepository
        {
            _services.AddTransient<IOutboxMessageRepository, T>();
        }

        public void WithDomainEventRepository(Func<IServiceProvider, IOutboxMessageRepository> implementationFactory)
        {
            _services.AddTransient(implementationFactory);
        }

        public void RegisterDomainEvent<TDomainEvent>(string topicName, string eventTypeName, Func<TDomainEvent, string> keySelector) where TDomainEvent : class
        {
            _registry.Register(topicName, eventTypeName, keySelector);
        }
    }
}