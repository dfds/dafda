using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Dafda.Producing
{
    public class OutgoingMessageRegistry : IOutgoingMessageRegistry
    {
        private readonly IDictionary<Type, OutgoingMessageRegistration> _registrations = new ConcurrentDictionary<Type, OutgoingMessageRegistration>();

        public void Register<T>(string topic, string type, Func<T, string> keySelector) where T : class
        {
            _registrations[typeof(T)] = new OutgoingMessageRegistration(topic, type, evt => keySelector((T) evt));
        }

        public OutgoingMessageRegistration GetRegistration(object @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            _registrations.TryGetValue(@event.GetType(), out var registration);
            return registration;
        }
    }
}