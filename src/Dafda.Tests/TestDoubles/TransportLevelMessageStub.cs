using System;
using Dafda.Consuming;

namespace Dafda.Tests.TestDoubles
{
    public class TransportLevelMessageStub : ITransportLevelMessage
    {
        public TransportLevelMessageStub(object data = null, string type = null)
        {
            Data = data;
            Type = type;
        }

        public string MessageId { get; set; }
        public string CorrelationId { get; set; }
        public string Type { get; set; }
        public object Data { get; set; }

        T ITransportLevelMessage.ReadDataAs<T>()
        {
            throw new NotImplementedException();
        }

        public object ReadDataAs(Type messageInstanceType)
        {
            return Data;
        }
    }
}