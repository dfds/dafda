using Dafda.Producing;

namespace Dafda.Tests.Builders
{
    internal class OutgoingMessageFactoryBuilder
    {
        private MessageIdGenerator _messageIdGenerator = MessageIdGenerator.Default;
        private IOutgoingMessageRegistry _outgoingMessageRegistry = new OutgoingMessageRegistry();

        public OutgoingMessageFactoryBuilder With(MessageIdGenerator messageIdGenerator)
        {
            _messageIdGenerator = messageIdGenerator;
            return this;
        }

        public OutgoingMessageFactoryBuilder With(IOutgoingMessageRegistry outgoingMessageRegistry)
        {
            _outgoingMessageRegistry = outgoingMessageRegistry;
            return this;
        }

        public OutgoingMessageFactoryBuilder With(OutgoingMessageRegistryBuilder builder)
        {
            _outgoingMessageRegistry = builder.Build();
            return this;
        }

        public OutgoingMessageFactory Build()
        {
            return new OutgoingMessageFactory(_messageIdGenerator, _outgoingMessageRegistry);
        }

        public static implicit operator OutgoingMessageFactory(OutgoingMessageFactoryBuilder builder)
        {
            return builder.Build();
        }
    }
}