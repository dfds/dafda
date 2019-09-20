using System;
using System.Collections;
using System.Collections.Generic;
using Dafda.Configuration;
using Dafda.Consuming;
using Dafda.Messaging;
using Dafda.Tests.Messaging;
using Dafda.Tests.TestDoubles;

namespace Dafda.Tests.Builders
{
    public class ConsumerBuilder
    {
        private IHandlerUnitOfWorkFactory _unitOfWorkFactory;
        private ITopicSubscriberScopeFactory _topicSubscriberScopeFactory;
        private MessageRegistration[] _messageRegistrations;

        public ConsumerBuilder()
        {
            _unitOfWorkFactory = new HandlerUnitOfWorkFactoryStub(null);

            var messageStub = new MessageResultBuilder().Build();
            _topicSubscriberScopeFactory = new TopicSubscriberScopeFactoryStub(new TopicSubscriberScopeStub(messageStub));

            _messageRegistrations = new MessageRegistration[0];
        }

        public ConsumerBuilder WithUnitOfWorkFactory(Func<Type, IHandlerUnitOfWork> unitOfWorkFactory)
        {
            _unitOfWorkFactory = new DefaultUnitOfWorkFactory(unitOfWorkFactory);
            return this;
        }

        public ConsumerBuilder WithTopicSubscriberScopeFactory(ITopicSubscriberScopeFactory topicSubscriberScopeFactory)
        {
            _topicSubscriberScopeFactory = topicSubscriberScopeFactory;
            return this;
        }

        public ConsumerBuilder WithMessageRegistrations(params MessageRegistration[] messageRegistrations)
        {
            _messageRegistrations = messageRegistrations;
            return this;
        }
        
        public Consumer Build()
        {
            var configuration = new ConsumerConfigurationStub
            {
                MessageHandlerRegistry = new MessageHandlerRegistryStub(_messageRegistrations),
                UnitOfWorkFactory = _unitOfWorkFactory,
                TopicSubscriberScopeFactory = _topicSubscriberScopeFactory
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
            public ITopicSubscriberScopeFactory TopicSubscriberScopeFactory { get; set; }
            public IEnumerable<string> SubscribedTopics { get; set; }
            public ICommitStrategy CommitStrategy { get; set; }
        }

        #endregion
    }
}