using Dafda.Consuming.Handlers;

namespace Dafda.Consuming
{
    /// <summary>
    /// Defines a strategy for dealing with messages that are not explicitly
    /// configured with a handler.
    /// Built-in strategies include RequireExplicitHandlers which will throw
    /// and UseNoOpHandler which will log that a message was received and continue
    /// </summary>
    public interface IUnconfiguredMessageHandlingStrategy
    {
        /// <summary>
        /// Either return a MessageRegistration providing a handler for this message type
        /// or explode horribly in some way I guess...
        /// </summary>
        MessageRegistration GetFallback(string messageType);
    }

    /// <summary>
    /// An unconfigured message handling strategy that will throw if no handler has been
    /// configured for a given message type
    /// </summary>
    public sealed class RequireExplicitHandlers : IUnconfiguredMessageHandlingStrategy
    {
        /// <summary>
        /// Throws an exception complaining that there is no handler for this message type
        /// </summary>
        public MessageRegistration GetFallback(string messageType) =>
            throw new MissingMessageHandlerRegistrationException(
                $"Error! A Handler has not been registered for messages of type {messageType}");
    }

    /// <summary>
    /// An unconfigured message handling strategy that will fallback to using the NoOpHandler
    /// </summary>
    public sealed class UseNoOpHandler : IUnconfiguredMessageHandlingStrategy
    {
        /// <summary>
        /// Returns a MessageRegistration specifying that this type should use the NoOpHandler
        /// </summary>
        public MessageRegistration GetFallback(string messageType) =>
            new MessageRegistration(
                typeof(NoOpHandler),
                typeof(object),
                "",
                messageType);

    }
}
