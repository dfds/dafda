using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Consuming;

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

    private class ServiceScopedUnitOfWork(IServiceProvider serviceProvider, Type handlerType) : IHandlerUnitOfWork
    {
        public async Task Run(Func<object, CancellationToken, Task> handlingAction, CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var handlerInstance = ActivatorUtilities.CreateInstance(scope.ServiceProvider, handlerType);
            await handlingAction(handlerInstance, cancellationToken);
        }
    }
}