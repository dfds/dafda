using System;
using Dafda.Outbox;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public sealed class OutboxOptions
    {
        private readonly IServiceCollection _services;
        private readonly OutgoingMessageRegistry _outgoingMessageRegistry;

        private MessageIdGenerator _messageIdGenerator = MessageIdGenerator.Default;

        internal OutboxOptions(IServiceCollection services, OutgoingMessageRegistry outgoingMessageRegistry)
        {
            _services = services;
            _outgoingMessageRegistry = outgoingMessageRegistry;
        }

        internal TimeSpan DispatchInterval { get; private set; } = TimeSpan.FromSeconds(5);

        public void WithMessageIdGenerator(MessageIdGenerator messageIdGenerator)
        {
            _messageIdGenerator = messageIdGenerator;
        }

        public void Register<T>(string topic, string type, Func<T, string> keySelector) where T : class
        {
            _outgoingMessageRegistry.Register(topic, type, keySelector);
        }

        public void WithOutboxMessageRepository<T>() where T : class, IOutboxMessageRepository
        {
            _services.AddTransient<IOutboxMessageRepository, T>();
        }

        public void WithOutboxMessageRepository(Func<IServiceProvider, IOutboxMessageRepository> implementationFactory)
        {
            _services.AddTransient(implementationFactory);
        }

        public void WithDispatchInterval(TimeSpan interval)
        {
            DispatchInterval = interval;
        }

        internal OutboxConfiguration Build()
        {
            return new OutboxConfiguration(_messageIdGenerator);
        }
    }
}