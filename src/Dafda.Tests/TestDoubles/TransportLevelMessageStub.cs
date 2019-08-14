using System;
using Dafda.Messaging;

namespace Dafda.Tests.TestDoubles
{
    public class TransportLevelMessageStub : ITransportLevelMessage
    {
        private readonly object _data;

        public TransportLevelMessageStub(object data = null, string type = null)
        {
            _data = data;
            Type = type;
        }

        public string MessageId { get; }
        public string Type { get; }
        public string CorrelationId { get; }

        T ITransportLevelMessage.ReadDataAs<T>()
        {
            throw new NotImplementedException();
        }

        public object ReadDataAs(Type messageInstanceType)
        {
            return _data;
        }
    }
}