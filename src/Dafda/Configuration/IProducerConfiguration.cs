using Dafda.Producing;

namespace Dafda.Configuration
{
    public interface IProducerConfiguration : IConfiguration
    {
        IMessageIdGenerator MessageIdGenerator { get; }
        IOutgoingMessageRegistry OutgoingMessageRegistry { get; }
    }
}