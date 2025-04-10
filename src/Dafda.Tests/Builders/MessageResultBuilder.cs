using System;
using System.Threading.Tasks;
using Dafda.Consuming;

namespace Dafda.Tests.Builders
{
    internal class MessageResultBuilder
    {
        private TransportLevelMessage _message = new TransportLevelMessageBuilder().WithType("foo").Build();
        private Func<Task> _onCommit;
        private string _topic = string.Empty;

        public MessageResultBuilder()
        {
            _onCommit = () => Task.CompletedTask;
        }

        public MessageResultBuilder WithTransportLevelMessage(TransportLevelMessage message)
        {
            _message = message;
            return this;
        }

        public MessageResultBuilder WithOnCommit(Func<Task> onCommit)
        {
            _onCommit = onCommit;
            return this;
        }

        public MessageResultBuilder WithTopic(string topic)
        {
            _topic = topic;
            return this;
        }

        public MessageResult Build()
        {
            return new MessageResult(_message, _onCommit)
            {
                Topic = _topic ?? string.Empty
            };
        }
    }
}