using System;
using System.Threading.Tasks;

namespace Dafda.Producing
{
    internal interface IKafkaProducer : IDisposable
    {
        Task Produce(OutgoingMessage outgoingMessage);
    }
}