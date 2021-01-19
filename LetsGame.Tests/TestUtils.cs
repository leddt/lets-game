using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace LetsGame.Tests
{
    public static class TestUtils
    {
        public static IConfiguration GetConfiguration(Dictionary<string, string> initialData)
        {
            return new ConfigurationRoot(new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(new MemoryConfigurationSource
                {
                    InitialData = initialData
                })
            });
        }
    }
}