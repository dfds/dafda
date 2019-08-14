using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Dafda.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
    internal abstract class MessageProcessingScope : IDisposable
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly ILogger<MessageProcessingScope> _logger;

        protected MessageProcessingScope(ILoggerFactory loggerFactory, IConsumer<string, string> consumer)
        {
            _consumer = consumer;
            _logger = loggerFactory.CreateLogger<MessageProcessingScope>();
        }

        protected abstract ILocalMessageDispatcher GetDispatcher();

        public virtual async Task Process(CancellationToken cancellationToken)
        {
            var consumeResult = _consumer.Consume(cancellationToken);

            _logger.LogDebug("Received: {Key} {Data}", consumeResult.Key, consumeResult.Value);

            var message = new JsonMessageEmbeddedDocument(consumeResult.Value);

            var dispatcher = GetDispatcher();
            await dispatcher.Dispatch(message);

            _consumer.Commit(consumeResult);
        }

        public void Dispose()
        {
            OnDispose();
        }

        protected abstract void OnDispose();
    }

    internal class ServiceProviderBasedMessageProcessingScope : MessageProcessingScope
    {
        public ServiceProviderBasedMessageProcessingScope(ILoggerFactory loggerFactory, IConsumer<string, string> consumer, IServiceProvider serviceProvider) : base(loggerFactory, consumer)
        {
            ServiceScope = serviceProvider.CreateScope();
        }

        protected IServiceScope ServiceScope { get; }

        protected override ILocalMessageDispatcher GetDispatcher()
        {
            return ServiceScope.ServiceProvider.GetRequiredService<ILocalMessageDispatcher>();
        }

        protected override void OnDispose()
        {
            ServiceScope.Dispose();
        }
    }
}