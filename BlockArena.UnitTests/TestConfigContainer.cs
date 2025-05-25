using Microsoft.Extensions.Configuration;

namespace BlockArena.UnitTests
{
    public static class TestConfigContainer
    {
        public static IConfiguration GetConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", false, true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}