using System;
using System.Text.RegularExpressions;

namespace Dafda.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class NamingConvention
    {
        /// <summary>Default value</summary>
        public static readonly NamingConvention Default = new NamingConvention();

        /// <summary>If specific custom naming convention is needed</summary>
        public static NamingConvention UseCustom(Func<string, string> converter)
        {
            return new NamingConvention(converter);
        }

        /// <summary>If specific environment style is needed</summary>
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

            actualKey = actualKey.ToUpperInvariant();

            return actualKey;
        }

        private readonly Func<string, string> _converter;

        private NamingConvention(Func<string, string> converter = null)
        {
            _converter = converter ?? (s => s);
        }

        /// <summary>Get the value for the input key</summary>
        public string GetKey(string key)
        {
            return _converter(key);
        }
    }
}