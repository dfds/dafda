using System.Threading.Tasks;

namespace Dafda.Producing
{
    public interface IProducer
    {
        Task Produce(OutgoingMessage outgoingMessage);
    }
}