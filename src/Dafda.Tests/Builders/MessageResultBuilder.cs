using Dafda.Consuming;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Tests.Builders
{
    internal class MessageResultBuilder
    {
        private TransportLevelMessage _message = new TransportLevelMessageBuilder().WithType("foo").Build();
        private Func<CancellationToken, Task> _onCommit;
        private string _topic = string.Empty;

        public MessageResultBuilder()
        {
            _onCommit = (_) => Task.CompletedTask;
        }

        public MessageResultBuilder WithTransportLevelMessage(TransportLevelMessage message)
        {
            _message = message;
            return this;
        }

        public MessageResultBuilder WithOnCommit(Func<CancellationToken, Task> onCommit)
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