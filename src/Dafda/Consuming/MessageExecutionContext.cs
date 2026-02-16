using System;

namespace Dafda.Consuming
{
    /// <summary>
    /// Provides context information for consumer execution strategies.
    /// </summary>
    public class MessageExecutionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageExecutionContext"/> class.
        /// </summary>
        /// <param name="messageInstance">The deserialized message instance.</param>
        /// <param name="messageMetadata">The metadata associated with the message</param>
        /// <param name="messageInstanceType">The type of the message instance.</param>
        public MessageExecutionContext(object messageInstance, Metadata messageMetadata, Type messageInstanceType)
        {
            MessageInstance = messageInstance;
            MessageMetadata = messageMetadata;
            MessageInstanceType = messageInstanceType;
        }

        /// <summary>
        /// Gets the deserialized message instance.
        /// </summary>
        public object MessageInstance { get; }

        /// <summary>
        /// Gets the metadata associated with the message.
        /// </summary>
        public Metadata MessageMetadata { get; }

        /// <summary>
        /// The type of the message.
        /// </summary>
        public Type MessageInstanceType { get; }
    }
}
