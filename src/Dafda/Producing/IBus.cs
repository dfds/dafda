using System;
using System.Threading.Tasks;

namespace Dafda.Producing
{
    public interface IBus : IDisposable
    {
        Task Publish<TMessage>(TMessage msg) where TMessage : IMessage;
    }
}