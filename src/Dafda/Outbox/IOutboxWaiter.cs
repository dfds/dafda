namespace Dafda.Outbox
{
    public interface IOutboxWaiter
    {
        void WakeUp();
        void Wait();
    }
}