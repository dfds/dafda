namespace Dafda.Consuming.Interfaces
{
    internal interface IConsumerScopeFactory<TResult>
    {
        IConsumerScope<TResult> CreateConsumerScope();
    }
}