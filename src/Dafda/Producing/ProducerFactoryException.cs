using System;
using Dafda.Configuration;

namespace Dafda.Producing
{
    /// <summary>
    /// Exception thrown when multiple identical producers are added via
    /// <see cref="ProducerServiceCollectionExtensions.AddProducerFor{TClient}"/>
    /// </summary>
    public sealed class ProducerFactoryException : Exception
    {
        internal ProducerFactoryException(string message) : base(message)
        {
        }
    }
}