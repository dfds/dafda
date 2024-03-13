﻿using Dafda.Consuming;

namespace Dafda.Tests.Builders
{
    public class TransportLevelMessageBuilder
    {
        private Metadata _metadata;

        private object _data;

        public TransportLevelMessageBuilder()
        {
            WithType("foo-type");
            _data = null;
        }

        public TransportLevelMessageBuilder WithType(string type, string traceparent = null, string tracestate = null)
        {
            _metadata = new Metadata()
            {
                MessageId = "foo-message-id",
                CorrelationId = "foo-correlation-id",
                Type = type
            };

            _metadata["traceparent"] = traceparent;
            _metadata["tracestate"] = tracestate;

            return this;
        }

        public TransportLevelMessageBuilder WithData(object data)
        {
            _data = data;
            return this;
        }

        public TransportLevelMessage Build()
        {
            return new TransportLevelMessage(_metadata, type => _data);
        }
    }
}