using Microsoft.Extensions.Configuration;

namespace LetsGame.Tests
{
    public static class TestConfiguration
    {
        private static readonly IConfigurationRoot Config;

        static TestConfiguration()
        {
            Config = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .AddEnvironmentVariables()
                .Build();
        }

        public static string Timezone => Config["timezone"];
    }
}