using System;

namespace Dafda.DomainEvents
{
    public interface ITypeResolver
    {
        object Resolve(Type instanceType);
    }
}