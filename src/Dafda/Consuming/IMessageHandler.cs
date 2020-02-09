using System.Threading.Tasks;

namespace Dafda.Consuming
{
    public interface IMessageHandler<T> where T : class, new()
    {
        Task Handle(T message, MessageHandlerContext context);
    }

    public sealed class MessageHandlerContext
    {
        internal MessageHandlerContext(string messageId, string messageType)
        {
            MessageId = messageId;
            MessageType = messageType;
        }
        
        public string MessageId { get; private set; }
        public string MessageType { get; private set; }
    }
}