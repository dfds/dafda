using Confluent.Kafka;

namespace Dafda.Consuming
{
    public class ConsumeResult
    {
        private readonly ConsumeResult<string, string> _result;

        internal ConsumeResult(ConsumeResult<string, string> result)
        {
            _result = result;
        }

        public string Value => _result.Value;
        
        internal ConsumeResult<string, string> InnerResult => _result;
    }
}