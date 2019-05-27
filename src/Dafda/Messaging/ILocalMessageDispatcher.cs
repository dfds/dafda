using System.Threading.Tasks;

namespace Dafda.Messaging
{
    public interface ILocalMessageDispatcher
    {
        Task Dispatch(ITransportLevelMessage message);
    }
}