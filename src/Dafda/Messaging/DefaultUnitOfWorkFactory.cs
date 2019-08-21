using System;

namespace Dafda.Messaging
{
    public class DefaultUnitOfWorkFactory : IHandlerUnitOfWorkFactory
    {
        private readonly ITypeResolver _typeResolver;

        public DefaultUnitOfWorkFactory(ITypeResolver typeResolver)
        {
            _typeResolver = typeResolver;
        }

        public IHandlerUnitOfWork CreateForHandlerType(Type handlerType)
        {
            return new DefaultUnitOfWork(_typeResolver, handlerType);
        }
    }
}