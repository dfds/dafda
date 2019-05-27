using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dafda.Messaging;
using Xunit;

namespace Dafda.Tests
{
    public class TestMessageHandlerRegistry
    {
        [Fact]
        public void returns_expected_registrations_when_initialized()
        {
            var sut = new MessageHandlerRegistry();
            Assert.Empty(sut.Registrations);
        }

        [Fact]
        public void returns_expected_registrations_when_registering_single_handler()
        {
            var sut = new MessageHandlerRegistry();
            var result = sut.Register<FooMessage, FooHandler>("dummy topic", "dummy message type");

            var expected = new MessageRegistration
            {
                Topic = "dummy topic",
                MessageType = "dummy message type",
                HandlerInstanceType = typeof(FooHandler),
                MessageInstanceType = typeof(FooMessage)
            };
            
            Assert.Equal(expected, result, new MessageRegistrationComparer());
        }

        #region helper classes

        private class MessageRegistrationComparer : IEqualityComparer<MessageRegistration>
        {
            public bool Equals(MessageRegistration x, MessageRegistration y)
            {
                return x.Topic == y.Topic &&
                       x.MessageType == y.MessageType &&
                       x.HandlerInstanceType == y.HandlerInstanceType &&
                       x.MessageInstanceType == y.MessageInstanceType;
            }

            public int GetHashCode(MessageRegistration obj)
            {
                throw new NotImplementedException();
            }
        }
        
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
