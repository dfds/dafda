using Dafda.Producing;

namespace Dafda.Tests.TestDoubles
{
    public class MessageIdGeneratorStub : IMessageIdGenerator
    {
        private readonly string _id;

        public MessageIdGeneratorStub(string id)
        {
            _id = id;
        }

        public string NextMessageId()
        {
            return _id;
        }
    }
}