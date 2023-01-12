using System;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Moq;
using Xunit;

namespace Dafda.Tests.Consuming
{
    public class TestLocalMessageDispatcher
    {
        [Fact]
        public async Task throws_expected_exception_when_dispatching_and_no_handler_has_been_registered()
        {
            var transportMessageStub = new TransportLevelMessageBuilder().Build();
            var emptyMessageHandlerRegistryStub = new MessageHandlerRegistry();
            var dummyFactory = new HandlerUnitOfWorkFactoryStub(null);

            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(emptyMessageHandlerRegistryStub)
                .WithHandlerUnitOfWorkFactory(dummyFactory)
                .Build();

            await Assert.ThrowsAsync<MissingMessageHandlerRegistrationException>(() => sut.Dispatch(transportMessageStub));
        }

        [Fact]
        public async Task throws_expected_exception_when_dispatching_and_unable_to_resolve_handler_instance()
        {
            var transportMessageStub = new TransportLevelMessageBuilder().WithType("foo").Build();
            var messageRegistrationStub = new MessageRegistrationBuilder().WithMessageType("foo").Build();
            var registry = new MessageHandlerRegistry();
            registry.Register(messageRegistrationStub);

            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(registry)
                .WithHandlerUnitOfWorkFactory(new HandlerUnitOfWorkFactoryStub(null))
                .Build();

            await Assert.ThrowsAsync<UnableToResolveUnitOfWorkForHandlerException>(() => sut.Dispatch(transportMessageStub));
        }

        [Fact]
        public async Task handler_is_invoked_as_expected_when_dispatching()
        {
            var mock = new Mock<IMessageHandler<object>>();

            var transportMessageDummy = new TransportLevelMessageBuilder().WithType("foo").Build();
            var registrationDummy = new MessageRegistrationBuilder().WithMessageType("foo").Build();
            var registry = new MessageHandlerRegistry();
            registry.Register(registrationDummy);
            
            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(registry)
                .WithHandlerUnitOfWork(new UnitOfWorkStub(mock.Object))
                .Build();

            await sut.Dispatch(transportMessageDummy);

            mock.Verify(x => x.Handle(It.IsAny<object>(), It.IsAny<MessageHandlerContext>()), Times.Once);
        }

        [Fact]
        public async Task handler_exceptions_are_thrown_as_expected()
        {
            var transportMessageDummy = new TransportLevelMessageBuilder().WithType("foo").Build();
            var registrationDummy = new MessageRegistrationBuilder().WithMessageType("foo").Build();
            var registry = new MessageHandlerRegistry();
            registry.Register(registrationDummy);
            
            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(registry)
                .WithHandlerUnitOfWork(new UnitOfWorkStub(new ErroneusHandler()))
                .Build();

            await Assert.ThrowsAsync<ExpectedException>(() => sut.Dispatch(transportMessageDummy));
        }
        
        [Fact]
        public async Task has_correct_topic_and_partition_key_available_in_message_handler_context()
        {
            const string dummyTopic = "dummy-topic";
            const string dummyPartitionKey = "dummy-partition-key";
            
            var mock = new Mock<IMessageHandler<object>>();

            var transportMessageDummy = new TransportLevelMessageBuilder().WithType("foo").WithTopic(dummyTopic).WithPartitionKey(dummyPartitionKey).Build();
            var registrationDummy = new MessageRegistrationBuilder().WithMessageType("foo").Build();
            var registry = new MessageHandlerRegistry();
            registry.Register(registrationDummy);
            
            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(registry)
                .WithHandlerUnitOfWork(new UnitOfWorkStub(mock.Object))
                .Build();

            await sut.Dispatch(transportMessageDummy);

            mock.Verify(x => x.Handle(
                It.IsAny<object>(),
                It.Is<MessageHandlerContext>(x => x.Topic==dummyTopic && x.PartitionKey==dummyPartitionKey)), 
                Times.Once);
        }
       
        #region private helper classes

        private class ExpectedException : Exception
        {

        }

        private class ErroneusHandler : IMessageHandler<object>
        {
            public Task Handle(object message, MessageHandlerContext context)
            {
                throw new ExpectedException();
            }
        }

        #endregion
    }
}