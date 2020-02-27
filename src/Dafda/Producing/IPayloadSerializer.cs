using System.Threading.Tasks;

namespace Dafda.Producing
{
    public interface IPayloadSerializer
    {
        string PayloadFormat { get; }

        Task<string> Serialize(PayloadDescriptor payloadDescriptor);
    }
}