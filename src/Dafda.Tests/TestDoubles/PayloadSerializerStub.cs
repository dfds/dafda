using System.Threading.Tasks;
using Dafda.Producing;

namespace Dafda.Tests.TestDoubles
{
    public class PayloadSerializerStub : IPayloadSerializer
    {
        private readonly string _result;

        public PayloadSerializerStub(string result, string format = "application/text")
        {
            _result = result;
            PayloadFormat = format;
        }

        public string PayloadFormat { get; }

        public Task<string> Serialize(PayloadDescriptor payloadDescriptor)
        {
            return Task.FromResult(_result);
        }
    }
}