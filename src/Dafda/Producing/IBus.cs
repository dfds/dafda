using System;
using System.Threading.Tasks;

namespace Dafda.Producing
{
    public interface IBus : IDisposable
    {
        Task Publish(object message);
    }
}