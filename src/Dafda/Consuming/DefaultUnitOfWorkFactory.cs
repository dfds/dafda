using System;

namespace Dafda.Consuming
{
    public class DefaultUnitOfWorkFactory : IHandlerUnitOfWorkFactory
    {
        private readonly Func<Type, IHandlerUnitOfWork> _unitOfWorkFactory;

        public DefaultUnitOfWorkFactory(ITypeResolver typeResolver) 
            : this(type => new DefaultUnitOfWork(typeResolver, type))
        {

        }

        public DefaultUnitOfWorkFactory(Func<Type, IHandlerUnitOfWork> unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public IHandlerUnitOfWork CreateForHandlerType(Type handlerType)
        {
            return _unitOfWorkFactory(handlerType);
        }
    }
}