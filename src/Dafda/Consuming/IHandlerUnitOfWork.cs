using System;
using System.Threading.Tasks;

namespace Dafda.Consuming
{
    public interface IHandlerUnitOfWork
    {
        Task Run(Func<object, Task> handlingAction);
    }
}