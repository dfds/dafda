using Xunit;

namespace Dafda.Tests.Helpers
{
    public static class AssertJson
    {
        public static void Equal(string expected, string actual)
        {
            static string TrimJson(string x) =>
                x.Replace("\n", "")
                    .Replace("\r", "")
                    .Replace("\t", "")
                    .RegExReplace(@"([{,}])\s+", "$1")
                    .RegExReplace(@"\s+([{,}])", "$1");

            Assert.Equal(TrimJson(expected), TrimJson(actual));
        }
    }
}