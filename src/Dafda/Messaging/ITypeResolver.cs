using System;

namespace Dafda.Messaging
{
    public interface ITypeResolver
    {
        object Resolve(Type instanceType);
    }

    internal class DefaultTypeResolver : ITypeResolver
    {
        public object Resolve(Type instanceType)
        {
            return Activator.CreateInstance(instanceType);
        }
    }
}