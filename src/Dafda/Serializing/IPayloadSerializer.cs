using System.Threading.Tasks;

namespace Dafda.Serializing
{
    /// <summary>
    /// Implementations must use the message payload and metadata in the
    /// <see cref="PayloadDescriptor"/> in order to create a string representation.
    /// The MIME type format should be described by the <see cref="PayloadFormat"/>
    /// property. 
    /// </summary>
    public interface IPayloadSerializer
    {
        /// <summary>
        /// The MIME type of the payload format
        /// </summary>
        string PayloadFormat { get; }

        /// <summary>
        /// Serialize the payload using the message and metadata in the
        /// <see cref="PayloadDescriptor"/>
        /// </summary>
        /// <param name="payloadDescriptor">The payload description</param>
        /// <returns>A string representation of the payload</returns>
        Task<string> Serialize(PayloadDescriptor payloadDescriptor);
    }
}