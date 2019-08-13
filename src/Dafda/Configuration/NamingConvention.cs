using System;
using System.Text.RegularExpressions;

namespace Dafda.Configuration
{
    public class NamingConvention
    {
        public static readonly NamingConvention Default = new NamingConvention();

        public static NamingConvention UseCustom(Func<string, string> converter)
        {
            return new NamingConvention(converter);
        }

        public static NamingConvention UseEnvironmentStyle(string prefix = null)
        {
            return new NamingConvention(key => ConvertToEnvironmentStyle(prefix, key));
        }

        private static string ConvertToEnvironmentStyle(string prefix, string key)
        {
            var actualKey = key;

            if (prefix != null)
            {
                actualKey = prefix + "_" + key;
            }

            actualKey = Regex.Replace(actualKey, "[-. \t]+", "_");

            actualKey = actualKey.ToUpper();

            return actualKey;
        }

        private readonly Func<string, string> _converter;

        private NamingConvention(Func<string, string> converter = null)
        {
            _converter = converter ?? (s => s);
        }

        public virtual string GetKey(string key)
        {
            return _converter(key);
        }
    }
}