using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration.Outbox;
using Dafda.Outbox;
using Dafda.Producing;
using Dafda.Serializing;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    /// <summary>
    /// Options for Outbox configuration
    /// </summary>
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

        /// <summary>
        /// Overwrite the default MessageIdGenerator
        /// </summary>
        /// <see cref="MessageIdGenerator.Default"/>
        /// <param name="messageIdGenerator">A custom implementation of MessageIdGenerator</param>
        public void WithMessageIdGenerator(MessageIdGenerator messageIdGenerator)
        {
            _messageIdGenerator = messageIdGenerator;
        }

        /// <summary>
        /// Register an outbox message of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="topic">The name of the topic</param>
        /// <param name="type">The name of the event message</param>
        /// <param name="keySelector">A <see cref="Func{TResult}"/> that should return
        /// the partition key.</param>
        /// <typeparam name="T">The message class</typeparam>
        public void Register<T>(string topic, string type, Func<T, string> keySelector) where T : class
        {
            _outgoingMessageRegistry.Register(topic, type, keySelector);
        }

        /// <summary>
        /// The custom implementation of <see cref="IOutboxEntryRepository"/> is used
        /// to load unpublished outbox entries (<see cref="OutboxEntry"/>) to be
        /// processed by the Dafda outbox dispatcher.
        /// </summary>
        /// <typeparam name="T">A custom implementation of <see cref="IOutboxEntryRepository"/></typeparam>
        public void WithOutboxEntryRepository<T>() where T : class, IOutboxEntryRepository
        {
            _services.AddTransient<IOutboxEntryRepository, T>();
        }

        /// <summary>
        /// The custom implementation of <see cref="IOutboxEntryRepository"/> is used
        /// to load unpublished outbox entries (<see cref="OutboxEntry"/>) to be
        /// processed by the Dafda outbox dispatcher.
        /// </summary>
        /// <param name="implementationFactory">A factory method that allows the use of
        /// <see cref="IServiceProvider"/> to deliver the <see cref="IOutboxEntryRepository"/>
        /// instance.</param>
        public void WithOutboxEntryRepository(Func<IServiceProvider, IOutboxEntryRepository> implementationFactory)
        {
            _services.AddTransient(implementationFactory);
        }

        /// <summary>
        /// The <paramref name="notifier"/> is returned after a successful call
        /// to <see cref="o:OutboxQueue.Enqueue"/>, which in turn enables the client
        /// to call the <see cref="IOutboxNotifier.Notify"/> to send notifications
        /// about new outbox message to the Dafda outbox dispatcher
        /// </summary>
        /// <param name="notifier">A custom implementation of <see cref="IOutboxNotifier"/></param>
        public void WithNotifier(IOutboxNotifier notifier)
        {
            _notifier = notifier;
        }

        /// <summary>
        /// Override the <see cref="DefaultPayloadSerializer"/> with a custom implementation
        /// </summary>
        /// <param name="payloadSerializer">A custom implementation of <see cref="DefaultPayloadSerializer"/></param>
        public void WithDefaultPayloadSerializer(IPayloadSerializer payloadSerializer)
        {
            WithDefaultPayloadSerializer(() => payloadSerializer);
        }

        /// <summary>
        /// Override the <see cref="DefaultPayloadSerializer"/> with a custom implementation
        /// </summary>
        /// <param name="payloadSerializerFactory">A factory method that returns a custom implementation
        /// of <see cref="DefaultPayloadSerializer"/>
        /// </param>
        public void WithDefaultPayloadSerializer(Func<IPayloadSerializer> payloadSerializerFactory)
        {
            _topicPayloadSerializerRegistry.SetDefaultPayloadSerializer(payloadSerializerFactory);
        }

        /// <summary>
        /// Override the <see cref="DefaultPayloadSerializer"/> with a custom implementation for
        /// the specified <paramref name="topic"/>
        /// </summary>
        /// <param name="topic">Name of the topic</param>
        /// <param name="payloadSerializer">A custom implementation of <see cref="DefaultPayloadSerializer"/></param>
        public void WithPayloadSerializer(string topic, IPayloadSerializer payloadSerializer)
        {
            WithPayloadSerializer(topic, () => payloadSerializer);
        }

        /// <summary>
        /// Override the <see cref="DefaultPayloadSerializer"/> with a custom implementation for
        /// the specified <paramref name="topic"/>
        /// </summary>
        /// <param name="topic">Name of the topic</param>
        /// <param name="payloadSerializerFactory">A factory method that returns a custom implementation
        /// of <see cref="DefaultPayloadSerializer"/>
        /// </param>
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