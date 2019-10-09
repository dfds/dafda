namespace Dafda.Outbox
{
    public interface IOutboxUnitOfWorkFactory
    {
        IOutboxUnitOfWork Begin();
    }
}