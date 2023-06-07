using Dafda.Configuration;
using Dafda.Configuration.ProducerConfigurations;
using Dafda.Tests.Builders;

namespace Dafda.Tests.Producing
{
    internal static class A
    {
        public static ProducerBuilder Producer => new ProducerBuilder();
        public static OutgoingMessageRegistryBuilder OutgoingMessageRegistry => new OutgoingMessageRegistryBuilder();
        public static ProducerConfigurationBuilder ValidProducerConfiguration => new ProducerConfigurationBuilder().WithBootstrapServers("dummy");
    }
}