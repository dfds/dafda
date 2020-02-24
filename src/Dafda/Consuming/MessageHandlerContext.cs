namespace Dafda.Consuming
{
    public sealed class MessageHandlerContext
    {
        private readonly Metadata _metadata;

        internal MessageHandlerContext(Metadata metadata)
        {
            _metadata = metadata;
        }

        public string MessageId => _metadata.MessageId;
        public string MessageType => _metadata.Type;

        public string this[string key] => _metadata[key];
    }
}