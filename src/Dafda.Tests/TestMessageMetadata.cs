using System;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace Dafda.Tests
{
    public class TestMessageMetadata
    {
        private readonly ITestOutputHelper _output;

        public TestMessageMetadata(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Fact]
        public void NAME()
        {
            var doc = JsonDocument.Parse(@"{""a"":""b"",""data"":""d""}");

            foreach (var property in doc.RootElement.EnumerateObject())
            {
                _output.WriteLine(property.Name);
                
            }
        }

    }
}