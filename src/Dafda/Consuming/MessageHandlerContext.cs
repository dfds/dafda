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
        [Obsolete("This field will be removed in a later version. Clients should not create new contexts but reuse the context from the handler.")]
        public static readonly MessageHandlerContext Empty = new MessageHandlerContext();

        private readonly Metadata _metadata;

        /// <summary>
        /// Initialize an instance of the MessageHandlerContext
        /// </summary>
        [Obsolete("This field will be removed in a later version. Clients should not create new contexts but reuse the context from the handler.")]
        public MessageHandlerContext() : this(new Metadata())
        {
        }

        /// <summary>
        /// Initialize an instance of the MessageHandlerContext
        /// </summary>
        public MessageHandlerContext(Metadata metadata)
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
        /// The message identification in a tracing system.
        /// Only used if tracing is enabled. Set automatically by Dafda.
        /// </summary>
        public virtual string TraceParent => _metadata.TraceParent;

        /// <summary>
        /// Access to message metadata values.
        /// </summary>
        /// <param name="key">A metadata name</param>
        public virtual string this[string key] => _metadata[key];
    }
}