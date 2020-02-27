using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dafda.Producing;
using Dafda.Tests.Builders;
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
            var headerEntryStub = KeyValuePair.Create<string, object>("foo", "bar");

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

        #region private helper classes

        private static class AssertJson
        {
            public static void Equal(string expected, string actual)
            {
                static string TrimJson(string x) =>
                    x.Replace("\n", "")
                        .Replace("\r", "")
                        .Replace("\t", "")
                        .RegExReplace(@"([{,}])\s+", "$1")
                        .RegExReplace(@"\s+([{,}])", "$1");

                Assert.Equal(TrimJson(expected), TrimJson(actual));
            }
        }

        #endregion
    }

    public static class RegExStringExtensions
    {
        public static string RegExReplace(this string text, string pattern, string replacement)
        {
            return Regex.Replace(text, pattern, replacement);
        }
    }
}