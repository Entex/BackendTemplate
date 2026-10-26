using BackendTemplate.Application;
using BackendTemplate.Infrastructure;
using BackendTemplate.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BackendTemplate.Tests;

public sealed class ConfigurationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short-key")]
    public void ProductionRejectsMissingOrShortSigningKeys(string? key)
    {
        using var services = CreateServices(Environments.Production, key);
        Assert.Throws<OptionsValidationException>(() => services.GetRequiredService<IOptions<JwtOptions>>().Value);
    }

    [Fact]
    public void DevelopmentCanGenerateTemporaryKey()
    {
        using var services = CreateServices(Environments.Development, null);
        Assert.True(services.GetRequiredService<IOptions<JwtOptions>>().Value.Key.Length >= 32);
    }

    private static ServiceProvider CreateServices(string environmentName, string? key)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:Provider"] = "Sqlite",
            ["ConnectionStrings:Database"] = "Data Source=:memory:",
            ["Jwt:Key"] = key
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddApplication();
        services.AddInfrastructure(configuration, new TestEnvironment { EnvironmentName = environmentName });
        return services.BuildServiceProvider();
    }

    private sealed class TestEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "BackendTemplate.Tests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
