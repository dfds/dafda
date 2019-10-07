using Dafda.Producing;

namespace Dafda.Configuration
{
    public interface IProducerConfiguration : IConfiguration
    {
        MessageIdGenerator MessageIdGenerator { get; }
        IOutgoingMessageRegistry OutgoingMessageRegistry { get; }
        IKafkaProducerFactory KafkaProducerFactory { get; }
    }
}