namespace Dafda.Configuration
{
    public abstract class ConfigurationSource
    {
        public static readonly ConfigurationSource Null = new NullConfigurationSource();

        public abstract string GetByKey(string keyName);

        #region Null Object

        private class NullConfigurationSource : ConfigurationSource
        {
            public override string GetByKey(string keyName)
            {
                return null;
            }
        }

        #endregion
    }
}