var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BackendTemplate_Api>("api", launchProfileName: "http")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
    .WithHttpHealthCheck("/health/ready");

builder.Build().Run();
