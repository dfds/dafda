using System;

namespace Dafda.Consuming
{
    /// <summary>
    /// Contains metadata for a message.
    /// </summary>
    public class MessageHandlerContext
    {
        /// <summary>
        /// An empty MessageHandlerContext
        /// </summary>
        [Obsolete("This field is obsolete. Clients should not create new contexts but reuse the context from the handler.")]
        public static readonly MessageHandlerContext Empty = new MessageHandlerContext();

        private readonly Metadata _metadata;

        /// <summary>
        /// Initialize an instance of the MessageHandlerContext
        /// </summary>
        [Obsolete("Clients should not create new contexts but reuse the context from the handler.")]
        public MessageHandlerContext() : this(new Metadata())
        {
        }

        internal MessageHandlerContext(Metadata metadata)
        {
            _metadata = metadata;
        }

        /// <summary>
        /// The message identifier.
        /// </summary>
        public virtual string MessageId => _metadata.MessageId;

        /// <summary>
        /// The message type.
        /// </summary>
        public virtual string MessageType => _metadata.Type;

        /// <summary>
        /// The message correlation identifier.
        /// </summary>
        public virtual string CorrelationId => _metadata.CorrelationId;

        /// <summary>
        /// The message causation identifier.
        /// </summary>
        public virtual string CausationId => _metadata.CausationId;

        /// <summary>
        /// Access to message metadata values.
        /// </summary>
        /// <param name="key">A metadata name</param>
        public virtual string this[string key] => _metadata[key];
    }
}