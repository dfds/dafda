using System;
using Dafda.Messaging;

namespace Dafda.Tests.TestDoubles
{
    public class TypeResolverStub : ITypeResolver
    {
        private readonly object _result;

        public TypeResolverStub(object result)
        {
            _result = result;
        }

        public object Resolve(Type instanceType)
        {
            return _result;
        }
    }
}