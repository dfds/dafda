namespace Dafda.Outbox
{
    public interface IOutboxNotification : IOutboxNotifier
    {
        void Wait();
    }
}