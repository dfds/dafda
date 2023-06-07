using System.Threading.Tasks;
using Dafda.Consuming.Handlers;

namespace Dafda.Consuming.Interfaces
{
    /// <summary>
    /// A registration (see <see cref="MessageHandlerRegistry"/>) of <see cref="IMessageHandler{T}"/>
    /// will allow Dafda to redirect consumed messages to the concrete message handler implementation. 
    /// </summary>
    /// <typeparam name="T">The message type</typeparam>
    public interface IMessageHandler<T>
    {
        /// <summary>
        /// Consumed message of <typeparamref name="T"/> are passed to the concrete
        /// <see cref="IMessageHandler{T}"/> implementation, along with a message
        /// contextual data.
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="context">The message context</param>
        /// <returns><see cref="Task"/></returns>
        Task Handle(T message, MessageHandlerContext context);
    }
}
