using System;
using Dafda.Consuming;

namespace Dafda.Tests.TestDoubles
{
    public class TransportLevelMessageStub : ITransportLevelMessage
    {
        public TransportLevelMessageStub()
        {
        }

        public TransportLevelMessageStub(string type)
        {
            Metadata.Type = type;
        }

        public Metadata Metadata { get; set; } = new Metadata();
        public object Data { get; set; }

        public object ReadDataAs(Type messageInstanceType)
        {
            return Data;
        }
    }
}