using System;
using System.Threading.Tasks;

namespace Dafda.Producing
{
    public interface IKafkaProducer : IDisposable
    {
        Task Produce(OutgoingMessage outgoingMessage);
    }
}