using Dafda.Producing;

namespace Dafda.Configuration
{
    public interface IProducerConfiguration : IConfiguration
    {
        MessageIdGenerator MessageIdGenerator { get; }
        IOutgoingMessageRegistry OutgoingMessageRegistry { get; }

        IKafkaProducer CreateKafkaProducer();
        OutgoingMessageFactory CreateOutgoingMessageFactory();
    }
}