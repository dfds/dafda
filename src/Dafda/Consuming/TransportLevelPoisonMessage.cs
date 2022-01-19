using System;

namespace Dafda.Consuming
{
    /// <summary>
    /// A message that could not be deserialized
    /// </summary>
    public class TransportLevelPoisonMessage
    {
        /// <summary>
        /// Friendly type name
        /// </summary>
        public static string Type => "transport-level-poison-message";

        /// <summary>
        /// The raw message as retreived from Kafka
        /// </summary>
        public string RawMessage { get; }

        /// <summary>
        /// The exception thrown when trying to deserialize
        /// </summary>
        public Exception DeserializationException { get; }

        /// <summary>
        /// Create a new instance of <see cref="TransportLevelPoisonMessage"/>
        /// </summary>
        /// <param name="rawMessage"></param>
        /// <param name="deserializationException"></param>
        public TransportLevelPoisonMessage(string rawMessage, Exception deserializationException)
        {
            RawMessage = rawMessage;
            DeserializationException = deserializationException;
        }
    }
}
