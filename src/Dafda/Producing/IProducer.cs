using System;
using System.Threading.Tasks;

namespace Dafda.Producing
{
    public interface IProducer : IDisposable
    {
        Task Produce(object message);
    }
}