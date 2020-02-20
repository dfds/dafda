using System;

namespace Dafda.Consuming
{
    internal interface ITypeResolver
    {
        object Resolve(Type instanceType);
    }
}