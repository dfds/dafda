using System;
using Dafda.Producing;

namespace Dafda.Tests.Builders
{
    internal class OutgoingMessageRegistryBuilder
    {
        private readonly OutgoingMessageRegistry _outgoingMessageRegistry = new OutgoingMessageRegistry();

        public OutgoingMessageRegistryBuilder Register<T>(string topic, string type, Func<T, string> keySelector) where T : class
        {
            _outgoingMessageRegistry.Register(topic, type, keySelector);
            return this;
        }

        public OutgoingMessageRegistry Build()
        {
            return _outgoingMessageRegistry;
        }

        public static implicit operator OutgoingMessageRegistry(OutgoingMessageRegistryBuilder builder)
        {
            return builder.Build();
        }
    }
}