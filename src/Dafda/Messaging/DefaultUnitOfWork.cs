using System;
using System.Threading.Tasks;

namespace Dafda.Messaging
{
    public class DefaultUnitOfWork : IHandlerUnitOfWork
    {
        private readonly ITypeResolver _typeResolver;
        private readonly Type _handlerType;

        public DefaultUnitOfWork(ITypeResolver typeResolver, Type handlerType)
        {
            _typeResolver = typeResolver;
            _handlerType = handlerType;
        }

        public async Task Run(Func<object, Task> handlingAction)
        {
            var handler = _typeResolver.Resolve(_handlerType);
            await handlingAction(handler);
        }
    }
}