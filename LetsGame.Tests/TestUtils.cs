using System.Collections.Generic;
using LetsGame.Web.Data;
using Microsoft.EntityFrameworkCore;
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

        public static ApplicationDbContext GetInMemoryDbContext()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("tests")
                .Options;
            
            return new ApplicationDbContext(dbContextOptions);
        }
    }
}