using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Messaging
{
    internal class ServiceProviderUnitOfWorkFactory : IHandlerUnitOfWorkFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderUnitOfWorkFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IHandlerUnitOfWork CreateForHandlerType(Type handlerType)
        {
            return new ServiceScopedUnitOfWork(_serviceProvider, handlerType, _serviceProvider.GetRequiredService<ScopedUnitOfWork>());
        }

        private class ServiceScopedUnitOfWork : IHandlerUnitOfWork
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly Type _handlerType;
            private readonly ScopedUnitOfWork _unitOfWork;

            public ServiceScopedUnitOfWork(IServiceProvider serviceProvider, Type handlerType, ScopedUnitOfWork unitOfWork)
            {
                _serviceProvider = serviceProvider;
                _handlerType = handlerType;
                _unitOfWork = unitOfWork;
            }

            public Task Run(Func<object, Task> handlingAction)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var handlerInstance = scope.ServiceProvider.GetRequiredService(_handlerType);

                    return _unitOfWork.ExecuteInScope(scope, () => handlingAction(handlerInstance));
                }
            }
        }
    }
}