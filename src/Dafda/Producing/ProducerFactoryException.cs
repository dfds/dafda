using System;
using Dafda.Configuration;

namespace Dafda.Producing
{
    /// <summary>
    /// Exception thrown when the service type used to identify a Dafda producer is already present in the
    /// <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> at the time
    /// <see cref="ProducerServiceCollectionExtensions.AddProducerFor{TClient}(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Dafda.Configuration.ProducerOptions})"/>,
    /// <see cref="ProducerServiceCollectionExtensions.AddProducerFor{TService,TImplementation}(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Dafda.Configuration.ProducerOptions})"/>,
    /// <see cref="ProducerServiceCollectionExtensions.AddProducerFor{TClient}(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{System.IServiceProvider,Dafda.Configuration.ProducerOptions})"/>, or
    /// <see cref="ProducerServiceCollectionExtensions.AddProducerFor{TService,TImplementation}(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{System.IServiceProvider,Dafda.Configuration.ProducerOptions})"/>
    /// is called. Each Dafda producer requires a service type that is not yet registered — the conflicting
    /// registration may come from another <c>AddProducerFor</c> call or from any other source.
    /// </summary>
    public sealed class ProducerFactoryException : Exception
    {
        internal ProducerFactoryException(string message) : base(message)
        {
        }
    }
}