using System;

namespace Dafda.Consuming
{
    public interface ITypeResolver
    {
        object Resolve(Type instanceType);
    }
}