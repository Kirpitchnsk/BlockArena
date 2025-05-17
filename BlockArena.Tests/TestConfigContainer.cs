using System.IO;
using Microsoft.Extensions.Configuration;

namespace BlockArena.Tests
{
    public static class TestConfigContainer
    {
        public static IConfiguration GetConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}