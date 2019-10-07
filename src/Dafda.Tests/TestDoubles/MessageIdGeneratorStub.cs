using System;
using Dafda.Producing;

namespace Dafda.Tests.TestDoubles
{
    public class MessageIdGeneratorStub : MessageIdGenerator
    {
        private readonly Func<string> _idGenerator;

        public MessageIdGeneratorStub(Func<string> idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public override string NextMessageId()
        {
            return _idGenerator();
        }
    }
}