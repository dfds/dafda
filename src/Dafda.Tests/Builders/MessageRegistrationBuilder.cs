using System;
using System.Threading.Tasks;
using Dafda.Consuming;

namespace Dafda.Tests.Builders
{
    public class MessageRegistrationBuilder
    {
        private string _topic;
        private string _messageType;
        private Type _handlerInstanceType;
        private Type _messageInstanceType;

        public MessageRegistrationBuilder()
        {
            _topic = "dummy topic";
            _messageType = "dummy message type";
            _handlerInstanceType = typeof(FooHandler);
            _messageInstanceType = typeof(FooMessage);
        }

        public MessageRegistrationBuilder WithTopic(string topic)
        {
            _topic = topic;
            return this;
        }

        public MessageRegistrationBuilder WithMessageType(string messageType)
        {
            _messageType = messageType;
            return this;
        }

        public MessageRegistrationBuilder WithHandlerInstanceType(Type handlerInstanceType)
        {
            _handlerInstanceType = handlerInstanceType;
            return this;
        }

        public MessageRegistrationBuilder WithMessageInstanceType(Type messageInstanceType)
        {
            _messageInstanceType = messageInstanceType;
            return this;
        }

        public MessageRegistration Build()
        {
            return new MessageRegistration(
                topic: _topic,
                messageType: _messageType,
                handlerInstanceType: _handlerInstanceType,
                messageInstanceType: _messageInstanceType
            );
        }

        #region private helper classes

        private class FooMessage
        {

        }

        private class FooHandler : IMessageHandler<FooMessage>
        {
            public Task Handle(FooMessage message)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}