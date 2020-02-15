namespace Dafda.Producing
{
    public interface IKafkaProducerFactory
    {
        IKafkaProducer CreateProducer();
    }
}