using System;

namespace Dafda.Producing
{
    public interface IOutgoingMessageRegistry
    {
        void Register<T>(string topic, string type, Func<T, string> keySelector) where T : class;
        OutgoingMessageRegistration GetRegistration(object @event);
    }
}