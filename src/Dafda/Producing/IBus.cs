using System.Threading.Tasks;

namespace Dafda.Producing
{
    public interface IBus
    {
        Task Publish<TMessage>(TMessage msg) where TMessage : IMessage;
    }
}