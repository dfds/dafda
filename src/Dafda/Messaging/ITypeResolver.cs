using System;

namespace Dafda.Messaging
{
    public interface ITypeResolver
    {
        object Resolve(Type instanceType);
    }
}