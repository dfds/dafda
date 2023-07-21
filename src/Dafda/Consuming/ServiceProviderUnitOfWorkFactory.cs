using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Consuming
{
    /// <summary>Default IHandlerUnitOfWorkFactory implemtation</summary>
    public class ServiceProviderUnitOfWorkFactory : IHandlerUnitOfWorkFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>Constructor</summary>
        public ServiceProviderUnitOfWorkFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>Returns new ServiceScopedUnitOfWork for handler type</summary>
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