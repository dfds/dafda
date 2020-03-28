namespace Dafda.Consuming
{
    public class MessageHandlerContext
    {
        public static readonly MessageHandlerContext Empty = new MessageHandlerContext();
        
        private readonly Metadata _metadata;

        public MessageHandlerContext() : this(new Metadata())
        {
        }

        internal MessageHandlerContext(Metadata metadata)
        {
            _metadata = metadata;
        }

        public virtual string MessageId => _metadata.MessageId;
        public virtual string MessageType => _metadata.Type;

        public virtual string this[string key] => _metadata[key];
    }
}