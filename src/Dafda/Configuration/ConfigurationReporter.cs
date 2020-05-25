using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dafda.Configuration
{
    /// <summary>
    /// Base class for reporting configuration values
    /// </summary>
    public abstract class ConfigurationReporter
    {
        /// <summary>
        /// A null ConfigurationReporter, which does nothing 
        /// </summary>
        public static readonly ConfigurationReporter Null = new NullConfigurationReporter();

        #region Null Object

        private class NullConfigurationReporter : ConfigurationReporter
        {
            public override void AddMissing(string key, string source, params string[] attemptedKeys)
            {
            }

            public override void AddValue(string key, string source, string value, string acceptedKey)
            {
            }

            public override void AddManual(string key, string value)
            {
            }

            public override string Report()
            {
                return string.Empty;
            }
        }

        #endregion

        /// <summary>
        /// Create a default ConfigurationReporter, which renders configuration values as a string
        /// </summary>
        /// <returns>The configuration values report</returns>
        public static ConfigurationReporter CreateDefault() => new DefaultConfigurationReporter();

        /// <summary>
        /// Add an indication of a missing required configuration key.
        /// </summary>
        /// <param name="key">The configuration key</param>
        /// <param name="source">The configuration source</param>
        /// <param name="attemptedKeys">The attempted keys</param>
        public abstract void AddMissing(string key, string source, params string[] attemptedKeys);

        /// <summary>
        /// Add an indication of a configuration <paramref name="key"/> added from the
        /// configuration <paramref name="source"/> 
        /// </summary>
        /// <param name="key">The configuration key</param>
        /// <param name="source">The configuration source</param>
        /// <param name="value"></param>
        /// <param name="acceptedKey"></param>
        public abstract void AddValue(string key, string source, string value, string acceptedKey);

        /// <summary>
        /// Add an indication of a manually added key 
        /// </summary>
        /// <param name="key">The configuration key</param>
        /// <param name="value"></param>
        public abstract void AddManual(string key, string value);

        /// <summary>
        /// Render the configuration value report
        /// </summary>
        /// <returns>The configuration values report</returns>
        public abstract string Report();

        private class DefaultConfigurationReporter : ConfigurationReporter
        {
            private readonly IList<Item> _items = new List<Item>();

            private class Item
            {
                public Item(string key, string source, string value, string keys)
                {
                    Key = key;
                    Source = source;
                    Value = value;
                    Keys = keys;
                }

                public string Key { get; }
                public string Source { get; }
                public string Value { get; }
                public string Keys { get; }
            }

            public override void AddMissing(string key, string source, params string[] attemptedKeys)
            {
                _items.Add(new Item(key, source, "MISSING", string.Join(", ", attemptedKeys)));
            }

            public override void AddValue(string key, string source, string value, string acceptedKey)
            {
                _items.Add(new Item(key, source, value, acceptedKey));
            }

            public override void AddManual(string key, string value)
            {
                _items.Add(new Item(key, "MANUAL", value, ""));
            }

            public override string Report()
            {
                var sb = new StringBuilder();

                var headers = new[] {"key", "source", "value", "keys"};
                var rows = _items.Select(x => new[] {x.Key, x.Source, x.Value, x.Keys}).ToList();
                rows.Insert(0, headers);

                var maxWidths = new int[headers.Length];

                foreach (var row in rows)
                {
                    for (var i = 0; i < row.Length; i++)
                    {
                        maxWidths[i] = Math.Max(maxWidths[i], row[i]?.Length ?? 0);
                    }
                }

                var fMaxWidth = new string[headers.Length];

                for (var i = 0; i < maxWidths.Length; i++)
                {
                    var maxWidth = maxWidths[i];

                    if (i < 3)
                    {
                        fMaxWidth[i] = $"{{0,-{maxWidth}}}";
                    }
                    else
                    {
                        fMaxWidth[i] = "{0}";
                    }
                }

                rows.RemoveAt(0);

                sb.AppendLine();

                for (var i = 0; i < fMaxWidth.Length; i++)
                {
                    var fmt = fMaxWidth[i];
                    sb.AppendFormat(fmt, headers[i]);
                    if (i < 3)
                    {
                        sb.Append(" ");
                    }
                }

                sb.AppendLine();

                var s = new string('-', maxWidths.Sum() + 3 * 1);

                sb.AppendLine(s);

                foreach (string[] row in rows)
                {
                    for (var i = 0; i < fMaxWidth.Length; i++)
                    {
                        var fmt = fMaxWidth[i];
                        sb.AppendFormat(fmt, row[i]);
                        if (i < 3)
                        {
                            sb.Append(" ");
                        }
                    }

                    sb.AppendLine();
                }

                return sb.ToString();
            }
        }
    }
}