using System;
using System.Threading.Tasks;
using Dafda.Consuming.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Consuming.Factories
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
            return new ServiceScopedUnitOfWork(_serviceProvider, handlerType);
        }

        private class ServiceScopedUnitOfWork : IHandlerUnitOfWork
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly Type _handlerType;

            public ServiceScopedUnitOfWork(IServiceProvider serviceProvider, Type handlerType)
            {
                _serviceProvider = serviceProvider;
                _handlerType = handlerType;
            }

            public async Task Run(Func<object, Task> handlingAction)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var handlerInstance = scope.ServiceProvider.GetRequiredService(_handlerType);

                    await handlingAction(handlerInstance);
                }
            }
        }
    }
}