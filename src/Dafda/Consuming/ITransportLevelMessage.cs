using System;

namespace Dafda.Consuming
{
    public interface IIncomingMessageFactory
    {
        ITransportLevelMessage Create(string rawMessage);
    }

    internal class IncomingMessageFactory : IIncomingMessageFactory
    {
        public ITransportLevelMessage Create(string rawMessage)
        {
            return new JsonMessageEmbeddedDocument(rawMessage);
        }
    }

    public interface ITransportLevelMessage
    {
        Metadata Metadata { get; }

        object ReadDataAs(Type messageInstanceType);
    }
}