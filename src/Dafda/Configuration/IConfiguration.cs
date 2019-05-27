using System.Collections.Generic;

namespace Dafda.Configuration
{
    public interface IConfiguration : IEnumerable<KeyValuePair<string, string>>
    {
        
    }
}