using System;
using System.Threading.Tasks;

namespace Dafda.Messaging
{
    public interface IHandlerUnitOfWork
    {
        Task Run(Func<object, Task> handlingAction);
    }
}