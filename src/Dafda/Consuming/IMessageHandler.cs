using System.Threading.Tasks;

namespace Dafda.Consuming
{
    public interface IMessageHandler<T> where T : class, new()
    {
        Task Handle(T message, MessageHandlerContext context);
    }

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