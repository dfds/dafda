using System.Threading.Tasks;

namespace Dafda.DomainEvents
{
    public interface IDomainEventHandler<T> where T : IDomainEvent
    {
        Task Handle(T message);
    }
}