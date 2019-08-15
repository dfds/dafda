using System;
using System.Collections.Generic;
using System.Linq;
using Dafda.Messaging;

namespace Dafda.Tests.TestDoubles
{
    public class MessageHandlerRegistryStub : IMessageHandlerRegistry
    {
        private readonly MessageRegistration[] _result;

        public MessageHandlerRegistryStub(params MessageRegistration[] result)
        {
            _result = result;
        }

        public MessageRegistration Register(Type handlerInstanceType, Type messageInstanceType, string topic, string messageType)
        {
            throw new NotImplementedException();
        }

        public MessageRegistration Register<TMessage, THandler>(string topic, string messageType) where TMessage : class, new() where THandler : IMessageHandler<TMessage>
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MessageRegistration> Registrations => _result;

        public MessageRegistration GetRegistrationFor(string messageType)
        {
            return _result.FirstOrDefault();
        }
    }
}