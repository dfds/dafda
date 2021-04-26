using System.Text.RegularExpressions;

namespace Dafda.Tests.Helpers
{
    public static class RegExStringExtensions
    {
        public static string RegExReplace(this string text, string pattern, string replacement)
        {
            return Regex.Replace(text, pattern, replacement);
        }
    }
}