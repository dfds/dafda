namespace Dafda.Configuration
{
    public interface IConfigurationProvider
    {
        string GetByKey(string keyName);
    }
}