using System;

namespace Dafda.Configuration
{
    public abstract class ConfigurationProvider
    {
        public static readonly ConfigurationProvider Null = new NullConfigurationProvider();

        public abstract string GetByKey(string keyName);

        #region Null Object

        private class NullConfigurationProvider : ConfigurationProvider
        {
            public override string GetByKey(string keyName)
            {
                return null;
            }
        }

        #endregion
    }
}