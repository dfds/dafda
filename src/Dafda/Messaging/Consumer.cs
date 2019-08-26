using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;

namespace Dafda.Messaging
{
    public class Consumer
    {
        private readonly IConsumerConfiguration _configuration;
        private readonly IInternalConsumerFactory _consumerFactory;
        private readonly LocalMessageDispatcher _localMessageDispatcher;

        public Consumer(IConsumerConfiguration configuration)
        {
            _configuration = configuration;
            _localMessageDispatcher = new LocalMessageDispatcher(configuration.MessageHandlerRegistry, configuration.UnitOfWorkFactory);
            _consumerFactory = _configuration.InternalConsumerFactory;
        }

        public async Task ConsumeAll(CancellationToken cancellationToken)
        {
            using (var internalConsumer = _consumerFactory.CreateConsumer(_configuration))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await ProcessNextMessage(internalConsumer, cancellationToken);
                }
            }
        }

        public async Task ConsumeSingle(CancellationToken cancellationToken)
        {
            using (var internalConsumer = _consumerFactory.CreateConsumer(_configuration))
            {
                await ProcessNextMessage(internalConsumer, cancellationToken);
            }
        }

        private async Task ProcessNextMessage(IInternalConsumer internalConsumer, CancellationToken cancellationToken)
        {
            var consumeResult = internalConsumer.Consume(cancellationToken);
            var message = new JsonMessageEmbeddedDocument(consumeResult.Value);

            await _localMessageDispatcher.Dispatch(message);

            await consumeResult.Commit();
        }
    }
}