using System.Threading.Tasks;

namespace Dafda.Consuming
{
    public interface IMessageHandler<T> where T : class, new()
    {
        Task Handle(T message);
    }
}