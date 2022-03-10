using System;
using System.Threading.Tasks;
using Dafda.Configuration;

namespace Dafda.Consuming
{
    internal sealed class ConsumerErrorHandler
    {
        public static readonly ConsumerErrorHandler Default = new(_ => Task.FromResult(ConsumerFailureStrategy.Default));

        private readonly Func<Exception, Task<ConsumerFailureStrategy>> _eval;

        public ConsumerErrorHandler(Func<Exception, Task<ConsumerFailureStrategy>> eval)
        {
            _eval = eval;
        }

        public Task<ConsumerFailureStrategy> Handle(Exception exception)
        {
            return _eval(exception);
        }
    }
}