using System;

namespace Dafda.Configuration
{
    public interface IConfigurationProvider
    {
        string GetByKey(string keyName);
    }

    public abstract class ConfigurationProvider : IConfigurationProvider
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