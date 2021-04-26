using System.Collections.Generic;
using System.Threading.Tasks;
using Dafda.Serializing;
using Dafda.Tests.Builders;
using Dafda.Tests.Helpers;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestDefaultPayloadSerializer
    {
        [Fact]
        public async Task returns_expected_serialized_value_of_simple_payload_without_headers()
        {
            var sut = new DefaultPayloadSerializer();

            var messageDataStub = "foo-message-data";

            var payloadStub = new PayloadDescriptorBuilder()
                .WithMessageData(messageDataStub)
                .Build();

            AssertJson.Equal(
                expected: $@"
                            {{
                                ""messageId"":""{payloadStub.MessageId}"",
                                ""type"":""{payloadStub.MessageType}"",
                                ""data"":""{messageDataStub}""
                            }}",
                actual: await sut.Serialize(payloadStub)
            );
        }

        [Fact]
        public async Task returns_expected_serialized_value_of_complex_payload_without_headers()
        {
            var sut = new DefaultPayloadSerializer();

            var messageDataStub = new
            {
                Foo = "bar"
            };

            var payloadStub = new PayloadDescriptorBuilder()
                .WithMessageData(messageDataStub)
                .Build();

            AssertJson.Equal(
                expected: $@"
                            {{
                                ""messageId"":""{payloadStub.MessageId}"",
                                ""type"":""{payloadStub.MessageType}"",
                                ""data"":{{""foo"":""bar""}}
                            }}",
                actual: await sut.Serialize(payloadStub)
            );
        }

        [Fact]
        public async Task returns_expected_serialized_value_of_simple_payload_with_simple_headers()
        {
            var sut = new DefaultPayloadSerializer();

            var messageDataStub = "foo-message-data";
            var headerEntryStub = KeyValuePair.Create("foo", "bar");

            var payloadStub = new PayloadDescriptorBuilder()
                .WithMessageData(messageDataStub)
                .WithMessageHeaders(headerEntryStub)
                .Build();

            AssertJson.Equal(
                expected: $@"
                            {{
                                ""messageId"":""{payloadStub.MessageId}"",
                                ""type"":""{payloadStub.MessageType}"",
                                ""{headerEntryStub.Key}"":""{headerEntryStub.Value}"",
                                ""data"":""{messageDataStub}""
                            }}",
                actual: await sut.Serialize(payloadStub)
            );
        }
    }
}