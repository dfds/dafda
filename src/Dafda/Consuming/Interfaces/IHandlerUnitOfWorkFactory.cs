using System;

namespace Dafda.Consuming
{
    /// <summary>
    /// This factory is in charge of creating instances of
    /// <see cref="IHandlerUnitOfWork"/> for each call to
    /// <see cref="CreateForHandlerType"/>.
    /// </summary>
    public interface IHandlerUnitOfWorkFactory
    {
        /// <summary>
        /// Create a concrete instance of the <see cref="IHandlerUnitOfWork"/> based
        /// on the <paramref name="handlerType"/>.
        /// </summary>
        /// <param name="handlerType">The type of an <see cref="IMessageHandler{T}"/>
        /// implementation.</param>
        /// <returns>An instance of <see cref="IHandlerUnitOfWork"/>.</returns>
        IHandlerUnitOfWork CreateForHandlerType(Type handlerType);
    }
}