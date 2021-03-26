namespace Dafda.Tests
{
    public class MetadataEnvelope<T>
    {
        public string MessageId { get; set; }
        public string Type { get; set; }
        public string CausationId { get; set; }
        public string CorrelationId { get; set; }
        public T Data { get; set; }
    }
}