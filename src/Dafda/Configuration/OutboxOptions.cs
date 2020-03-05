using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Outbox;
using Dafda.Producing;
using Dafda.Serializing;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public sealed class OutboxOptions
    {
        private readonly IServiceCollection _services;
        private readonly OutgoingMessageRegistry _outgoingMessageRegistry;
        private readonly TopicPayloadSerializerRegistry _topicPayloadSerializerRegistry = new TopicPayloadSerializerRegistry(() => new DefaultPayloadSerializer());

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

        public void WithOutboxEntryRepository<T>() where T : class, IOutboxEntryRepository
        {
            _services.AddTransient<IOutboxEntryRepository, T>();
        }

        public void WithOutboxEntryRepository(Func<IServiceProvider, IOutboxEntryRepository> implementationFactory)
        {
            _services.AddTransient(implementationFactory);
        }

        public void WithNotifier(IOutboxNotifier notifier)
        {
            _notifier = notifier;
        }

        public void WithDefaultPayloadSerializer(IPayloadSerializer payloadSerializer)
        {
            WithDefaultPayloadSerializer(() => payloadSerializer);
        }

        public void WithDefaultPayloadSerializer(Func<IPayloadSerializer> payloadSerializerFactory)
        {
            _topicPayloadSerializerRegistry.SetDefaultPayloadSerializer(payloadSerializerFactory);
        }

        public void WithPayloadSerializer(string topic, IPayloadSerializer payloadSerializer)
        {
            WithPayloadSerializer(topic, () => payloadSerializer);
        }

        public void WithPayloadSerializer(string topic, Func<IPayloadSerializer> payloadSerializerFactory)
        {
            _topicPayloadSerializerRegistry.Register(topic, payloadSerializerFactory);
        }

        internal OutboxConfiguration Build()
        {
            return new OutboxConfiguration(_messageIdGenerator, _notifier, _topicPayloadSerializerRegistry);
        }

        private class DoNotNotify : IOutboxNotifier
        {
            public Task Notify(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}