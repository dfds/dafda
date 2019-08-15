using Dafda.Messaging;
using Dafda.Tests.TestDoubles;

namespace Dafda.Tests.Builders
{
    public class LocalMessageDispatcherBuilder
    {
        private IMessageHandlerRegistry _messageHandlerRegistry;
        private ITypeResolver _typeResolver;

        public LocalMessageDispatcherBuilder()
        {
            _messageHandlerRegistry = Dummy.Of<IMessageHandlerRegistry>();
            _typeResolver = Dummy.Of<ITypeResolver>();
        }

        public LocalMessageDispatcherBuilder WithMessageHandlerRegistry(IMessageHandlerRegistry messageHandlerRegistry)
        {
            _messageHandlerRegistry = messageHandlerRegistry;
            return this;
        }
        
        public LocalMessageDispatcher Build()
        {
            return new LocalMessageDispatcher(_messageHandlerRegistry, _typeResolver);
        }
    }
}