namespace Dafda.Consuming.Interfaces
{
    /// <summary>
    /// Interface for ConsumerScopeFactory
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface IConsumerScopeFactory<TResult>
    {
        /// <summary>
        /// Creates consumer scope with result type defined in TResult
        /// </summary>
        IConsumerScope<TResult> CreateConsumerScope();
    }
}