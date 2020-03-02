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
        private IOutboxNotifier _notifier = new DoNotNotify();

        internal OutboxOptions(IServiceCollection services, OutgoingMessageRegistry outgoingMessageRegistry)
        {
            _services = services;
            _outgoingMessageRegistry = outgoingMessageRegistry;
        }

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

        public void WithNotifier(IOutboxNotifier notifier)
        {
            _notifier = notifier;
        }

        internal OutboxConfiguration Build()
        {
            return new OutboxConfiguration(_messageIdGenerator, _notifier);
        }

        private class DoNotNotify : IOutboxNotifier
        {
            public void Notify()
            {
            }
        }
    }
}