using System;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Messaging;
using Dafda.Tests.TestDoubles;

namespace Dafda.Tests.Builders
{
    public class MessageResultBuilder
    {
        private ITransportLevelMessage _message;
        private Func<Task> _onCommit;

        public MessageResultBuilder()
        {
            _message = new TransportLevelMessageStub(type: "foo");
            _onCommit = () => Task.CompletedTask;
        }

        public MessageResultBuilder WithTransportLevelMessage(ITransportLevelMessage message)
        {
            _message = message;
            return this;
        }

        public MessageResultBuilder WithTransportLevelMessage(string messageType)
        {
            _message = new TransportLevelMessageStub(type: messageType);
            return this;
        }

        public MessageResultBuilder WithOnCommit(Func<Task> onCommit)
        {
            _onCommit = onCommit;
            return this;
        }

        public MessageResult Build()
        {
            return new MessageResult(_message, _onCommit);
        }
    }
}