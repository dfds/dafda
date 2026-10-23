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
            await handlingAction(CreateHandler(scope.ServiceProvider), cancellationToken);
        }

        private object CreateHandler(IServiceProvider scopedServiceProvider)
        {
            try
            {
                return ActivatorUtilities.CreateInstance(scopedServiceProvider, handlerType);
            }
            catch (Exception exception)
            {
                throw new InvalidMessageHandlerException($"Error! Unable to create message handler of type \"{handlerType.FullName}\".", exception);
            }
        }
    }
}