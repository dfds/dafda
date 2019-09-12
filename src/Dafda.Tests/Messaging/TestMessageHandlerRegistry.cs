using System;
using System.Collections.Generic;
using Dafda.Messaging;
using Dafda.Tests.Builders;
using Xunit;

namespace Dafda.Tests.Messaging
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

            var expected = new MessageRegistrationBuilder().Build();

            var result = sut.Register(
                handlerInstanceType: expected.HandlerInstanceType,
                messageInstanceType: expected.MessageInstanceType,
                topic: expected.Topic,
                messageType: expected.MessageType
            );

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

        #endregion
    }
}
