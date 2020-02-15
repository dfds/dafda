using System;
using Dafda.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public sealed class OutboxPublisherOptions
    {
        private readonly IServiceCollection _services;

        internal OutboxPublisherOptions(IServiceCollection services)
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