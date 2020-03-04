using System.Collections.Generic;
using System.Collections.Immutable;

namespace Outbox.Domain
{
    public interface IDomainEvents
    {
        void Raise<TEvent>(TEvent @event);
    }

    public class DomainEvents : IDomainEvents
    {
        private readonly IList<object> _events = new List<object>();

        public void Raise<TEvent>(TEvent @event)
        {
            if (@event == null)
            {
                return;
            }

            _events.Add(@event);
        }

        public IEnumerable<object> Events => _events.ToImmutableList();
    }
}