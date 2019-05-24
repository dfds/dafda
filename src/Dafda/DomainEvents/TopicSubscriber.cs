using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Consuming;

namespace Dafda.DomainEvents
{
    public class TopicSubscriber
    {
        private readonly KafkaConsumerFactory _consumerFactory;
        private readonly MessageProcessor _messageProcessor;

        public TopicSubscriber(KafkaConsumerFactory consumerFactory, MessageProcessor messageProcessor)
        {
            _consumerFactory = consumerFactory;
            _messageProcessor = messageProcessor;
        }
        
        public async Task Start(IEnumerable<string> topicNames, CancellationToken cancellationToken)
        {
            using (var consumer = _consumerFactory.Create())
            {
                consumer.Subscribe(topicNames);

                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var consumeResult = consumer.Consume(cancellationToken);

                        await _messageProcessor.Process(new MessageEmbeddedDocument(consumeResult.Value));
                        consumer.Commit(consumeResult);
                    }
                }
                finally
                {
                   consumer.Close();
                }
            }
        }
    }
}