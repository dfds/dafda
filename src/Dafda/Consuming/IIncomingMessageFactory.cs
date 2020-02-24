namespace Dafda.Consuming
{
    public interface IIncomingMessageFactory
    {
        TransportLevelMessage Create(string rawMessage);
    }
}