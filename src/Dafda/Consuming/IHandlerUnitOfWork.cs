using System;
using System.Threading.Tasks;

namespace Dafda.Consuming
{
    /// <summary>
    /// The <see cref="IHandlerUnitOfWork"/> must provide the concrete instance
    /// of <see cref="IMessageHandler{T}"/>, based on the type provided in the
    /// <see cref="IHandlerUnitOfWorkFactory.CreateForHandlerType"/>, as the
    /// argument to <see cref="Run"/>.
    /// </summary>
    public interface IHandlerUnitOfWork
    {
        /// <summary>
        /// Ensures that the <paramref name="handlingAction"/> is run inside the scope
        /// of the <see cref="IHandlerUnitOfWork"/> implementation.
        /// </summary>
        /// <param name="handlingAction">The action to run.</param>
        /// <returns><see cref="Task"/></returns>
        Task Run(Func<object, Task> handlingAction);
    }
}