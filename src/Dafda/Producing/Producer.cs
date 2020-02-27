using System.Threading.Tasks;
using System.Collections.Generic;

namespace Dafda.Producing
{
    public sealed class Producer
    {
        private readonly KafkaProducer _kafkaProducer;
        private readonly PayloadDescriptorFactory _payloadDescriptorFactory;

        internal Producer(KafkaProducer kafkaProducer, OutgoingMessageRegistry outgoingMessageRegistry, MessageIdGenerator messageIdGenerator)
        {
            _kafkaProducer = kafkaProducer;
            _payloadDescriptorFactory = new PayloadDescriptorFactory(outgoingMessageRegistry, messageIdGenerator);
        }

        internal string Name { get; set; } = "__Default Producer__";
        
        public async Task Produce(object message)
        {
            await Produce(message, new Dictionary<string, object>());
        }

        public async Task Produce(object message, Dictionary<string, object> headers)
        {
            var payloadDescriptor = _payloadDescriptorFactory.Create(message, headers);
            await _kafkaProducer.Produce(payloadDescriptor);
        }
    }
}