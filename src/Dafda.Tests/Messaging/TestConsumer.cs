using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
using Dafda.Messaging;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Moq;
using Xunit;

namespace Dafda.Tests.Messaging
{
    public class TestConsumer
    {
        [Fact]
        public async Task invokes_expected_handler_when_consuming()
        {
            var handlerMock = new Mock<IMessageHandler<FooMessage>>();
            var handlerStub = handlerMock.Object;

            var messageRegistrationStub = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerStub.GetType())
                .WithMessageInstanceType(typeof(FooMessage))
                .WithMessageType("foo")
                .Build();

            var sut = new ConsumerBuilder()
                .WithUnitOfWorkFactory(type => new DirectUnitOfWork(handlerStub))
                .WithMessageRegistrations(messageRegistrationStub)
                .Build();

            await sut.ConsumeSingle(CancellationToken.None);

            handlerMock.Verify(x => x.Handle(It.IsAny<FooMessage>()), Times.Once);
        }

        [Fact]
        public async Task throws_expected_exception_when_consuming_a_message_without_a_handler_as_been_registered_for_it()
        {
            var sut = new ConsumerBuilder().Build();

            await Assert.ThrowsAsync<MissingMessageHandlerRegistrationException>(() => sut.ConsumeSingle(CancellationToken.None));
        }

        #region helper classes

        public class FooMessage
        {
            public string Value { get; set; }
        }

        #endregion
    }

    public class ConsumerBuilder
    {
        private string _groupId;
        private string _bootstrapServers;
        private IHandlerUnitOfWorkFactory _unitOfWorkFactory;
        private IInternalConsumerFactory _internalConsumerFactory;
        private MessageRegistration[] _messageRegistrations;

        public ConsumerBuilder()
        {
            _groupId = "foo";
            _bootstrapServers = "bar";
            _unitOfWorkFactory = new HandlerUnitOfWorkFactoryStub(null);
            _internalConsumerFactory = new InternalConsumerFactoryStub(new InternalConsumerStub("foo"));
            _messageRegistrations = new MessageRegistration[0];
        }

        public ConsumerBuilder WithUnitOfWorkFactory(Func<Type, IHandlerUnitOfWork> unitOfWorkFactory)
        {
            _unitOfWorkFactory = new DefaultUnitOfWorkFactory(unitOfWorkFactory);
            return this;
        }

        public ConsumerBuilder WithInternalConsumerFactory(IInternalConsumerFactory internalConsumerFactory)
        {
            _internalConsumerFactory = internalConsumerFactory;
            return this;
        }

        public ConsumerBuilder WithMessageRegistrations(params MessageRegistration[] messageRegistrations)
        {
            _messageRegistrations = messageRegistrations;
            return this;
        }
        
        public Consumer Build()
        {
            var handlerInstance = new object();

            var configuration = new ConsumerConfigurationStub
            {
                MessageHandlerRegistry = new MessageHandlerRegistryStub(_messageRegistrations),
                UnitOfWorkFactory = _unitOfWorkFactory,
                InternalConsumerFactory = _internalConsumerFactory
            };

            return new Consumer(configuration);
        }

        #region private helper classes

        private class ConsumerConfigurationStub : IConsumerConfiguration
        {
            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IMessageHandlerRegistry MessageHandlerRegistry { get; set; }
            public IHandlerUnitOfWorkFactory UnitOfWorkFactory { get; set; }
            public IInternalConsumerFactory InternalConsumerFactory { get; set; }
            public bool EnableAutoCommit { get; set; }
            public IEnumerable<string> SubscribedTopics { get; set; }
        }

        #endregion
    }
}