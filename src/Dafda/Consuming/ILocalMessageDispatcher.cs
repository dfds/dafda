using System.Threading.Tasks;

namespace Dafda.Consuming
{
    public interface ILocalMessageDispatcher
    {
        Task Dispatch(ITransportLevelMessage message);
    }
}