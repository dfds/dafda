using System.Threading.Tasks;
using Dafda.Producing;

namespace Dafda.Serializing
{
    public interface IPayloadSerializer
    {
        string PayloadFormat { get; }

        Task<string> Serialize(PayloadDescriptor payloadDescriptor);
    }
}