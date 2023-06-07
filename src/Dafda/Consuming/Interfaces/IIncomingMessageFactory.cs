namespace Dafda.Consuming.Interfaces
{
    /// <summary>
    /// The implementation of this interface will be in charge of deserializing the raw
    /// Kafka messages as they are consumed by the low-level Kafka consumer.
    /// </summary>
    public interface IIncomingMessageFactory
    {
        /// <summary>
        /// Deserialize the raw incoming Kafka message into a Dafda representation.
        /// </summary>
        /// <param name="rawMessage">The raw Kafka message, passed on by Dafda.</param>
        /// <returns>The deserialized version of the message.</returns>
        TransportLevelMessage Create(string rawMessage);
    }
}