using System.Threading.Tasks;

namespace Dafda.Messaging
{
    public interface IMessageHandler<T> where T : class, new()
    {
        Task Handle(T message);
    }
}