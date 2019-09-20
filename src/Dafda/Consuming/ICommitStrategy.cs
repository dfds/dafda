using System.Threading.Tasks;
using Confluent.Kafka;

namespace Dafda.Consuming
{
    public interface ICommitStrategy
    {
        Task Commit(IConsumer<string, string> consumer, ConsumeResult<string, string> innerResult);
    }

    public class AlwaysCommit : ICommitStrategy
    {
        public Task Commit(IConsumer<string, string> consumer, ConsumeResult<string, string> result)
        {
            consumer.Commit(result);
            return Task.CompletedTask;
        }
    }

    public class NeverCommit : ICommitStrategy
    {
        public Task Commit(IConsumer<string, string> consumer, ConsumeResult<string, string> innerResult)
        {
            return Task.CompletedTask;
        }
    }
}
