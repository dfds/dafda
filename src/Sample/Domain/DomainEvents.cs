using System.Collections.Generic;
using System.Collections.Immutable;

namespace Sample
{
    public class DomainEvents
    {
        private readonly IList<object> _events = new List<object>();

        public void Raise<TEvent>(TEvent @event)
        {
            _events.Add(@event);
        }

        public IEnumerable<object> Events => _events.ToImmutableList();
    }
}