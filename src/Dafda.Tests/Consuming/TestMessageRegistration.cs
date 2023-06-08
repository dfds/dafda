using System;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Consuming.Interfaces;
using Dafda.Tests.Builders;
using Xunit;

namespace Dafda.Tests.Consuming
{
    public class TestMessageRegistration
    {
        [Fact]
        public void returns_expected_handler_instance_type()
        {
            var expectedHandlerInstanceType = typeof(FooHandler);
            var messageInstanceTypeStub = typeof(FooMessage);

            var sut = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(expectedHandlerInstanceType)
                .WithMessageInstanceType(messageInstanceTypeStub)
                .Build();

            Assert.Equal(expectedHandlerInstanceType, sut.HandlerInstanceType);
        }

        [Fact]
        public void returns_expected_message_instance_type()
        {
            var handlerInstanceTypeStub = typeof(FooHandler);
            var expectedInstanceMessageType = typeof(FooMessage);

            var sut = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerInstanceTypeStub)
                .WithMessageInstanceType(expectedInstanceMessageType)
                .Build();

            Assert.Equal(expectedInstanceMessageType, sut.MessageInstanceType);
        }

        [Fact]
        public void returns_expected_message_topic()
        {
            var expectedTopic = "foo";

            var sut = new MessageRegistrationBuilder()
                .WithTopic(expectedTopic)
                .Build();

            Assert.Equal(expectedTopic, sut.Topic);
        }

        [Fact]
        public void throws_for_null_topic()
        {
            Assert.Throws<ArgumentException>(() =>
                new MessageRegistrationBuilder()
                .WithTopic(null)
                .Build());
        }

        [Fact]
        public void returns_expected_message_type()
        {
            var expectedMessageType = "foo";

            var sut = new MessageRegistrationBuilder()
                .WithMessageType(expectedMessageType)
                .Build();

            Assert.Equal(expectedMessageType, sut.MessageType);
        }

        [Fact]
        public void throws_exception_when_handler_closes_different_message_type_than_what_its_registered_with()
        {
            var handlerInstanceTypeStub = typeof(IMessageHandler<object>);
            var messageInstanceTypeStub = typeof(FooMessage);

            var builder = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerInstanceTypeStub)
                .WithMessageInstanceType(messageInstanceTypeStub);

            Assert.Throws<MessageRegistrationException>(() => builder.Build());
        }

        [Fact]
        public void throws_exception_when_handler_does_NOT_implement_expected_interface()
        {
            var invalidHandlerInstanceType = typeof(object);
            var builder = new MessageRegistrationBuilder().WithHandlerInstanceType(invalidHandlerInstanceType);

            Assert.Throws<MessageRegistrationException>(() => builder.Build());
        }

        #region helper classes

        private class FooMessage
        {

        }

        private class FooHandler : IMessageHandler<FooMessage>
        {
            public Task Handle(FooMessage message, MessageHandlerContext context)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}