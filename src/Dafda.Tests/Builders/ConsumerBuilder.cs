using System;
using System.Collections;
using System.Collections.Generic;
using Dafda.Configuration;
using Dafda.Consuming;
using Dafda.Messaging;
using Dafda.Tests.TestDoubles;

namespace Dafda.Tests.Builders
{
    public class ConsumerBuilder
    {
        private IHandlerUnitOfWorkFactory _unitOfWorkFactory;
        private IConsumerScopeFactory _consumerScopeFactory;
        private MessageRegistration[] _messageRegistrations;
        private bool _enableAutoCommit;

        public ConsumerBuilder()
        {
            _unitOfWorkFactory = new HandlerUnitOfWorkFactoryStub(null);

            var messageStub = new MessageResultBuilder().Build();
            _consumerScopeFactory = new ConsumerScopeFactoryStub(new ConsumerScopeStub(messageStub));

            _messageRegistrations = new MessageRegistration[0];
        }

        public ConsumerBuilder WithUnitOfWorkFactory(Func<Type, IHandlerUnitOfWork> unitOfWorkFactory)
        {
            _unitOfWorkFactory = new DefaultUnitOfWorkFactory(unitOfWorkFactory);
            return this;
        }

        public ConsumerBuilder WithConsumerScopeFactory(IConsumerScopeFactory consumerScopeFactory)
        {
            _consumerScopeFactory = consumerScopeFactory;
            return this;
        }

        public ConsumerBuilder WithMessageRegistrations(params MessageRegistration[] messageRegistrations)
        {
            _messageRegistrations = messageRegistrations;
            return this;
        }

        public ConsumerBuilder WithEnableAutoCommit(bool enableAutoCommit)
        {
            _enableAutoCommit = enableAutoCommit;
            return this;
        }

        public Consumer Build()
        {
            return new Consumer(
                messageHandlerRegistry: new MessageHandlerRegistryStub(_messageRegistrations),
                unitOfWorkFactory: _unitOfWorkFactory,
                consumerScopeFactory: _consumerScopeFactory,
                isAutoCommitEnabled: _enableAutoCommit
            );
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
            public IConsumerScopeFactory ConsumerScopeFactory { get; set; }
            public bool EnableAutoCommit { get; set; }
            public IEnumerable<string> SubscribedTopics { get; set; }
            public string GroupId { get; set; }
        }

        #endregion
    }
}