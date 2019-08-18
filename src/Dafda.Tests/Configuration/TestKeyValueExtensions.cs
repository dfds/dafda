using System.Collections.Generic;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestKeyValueExtensions
    {
        [Fact]
        public void Can_deconstruct_key_value_pair()
        {
            var dummy = new KeyValuePair<string, string>("foo", "bar");

            var (key, value) = dummy;

            Assert.Equal(dummy.Key, key);
            Assert.Equal(dummy.Value, value);
        }
    }
}