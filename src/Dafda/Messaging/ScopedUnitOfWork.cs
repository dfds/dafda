using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Messaging
{
    public class ScopedUnitOfWork
    {
        public virtual Task ExecuteInScope(IServiceScope scope, Func<Task> execute)
        {
            return execute();
        }
    }
}