using System;

namespace Dafda.Consuming
{
    internal class IncomingMessageFactory
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