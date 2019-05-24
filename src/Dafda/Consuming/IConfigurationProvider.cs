namespace Dafda.Consuming
{
    public interface IConfigurationProvider
    {
        string GetByKey(string keyName);
    }
}