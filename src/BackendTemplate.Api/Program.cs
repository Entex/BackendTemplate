using BackendTemplate.Api;
using BackendTemplate.Api.Endpoints;
using BackendTemplate.Application;
using BackendTemplate.Application.Behaviors;
using BackendTemplate.Application.Features.Users.Commands;
using BackendTemplate.Infrastructure;
using BackendTemplate.Infrastructure.Persistence;
using BackendTemplate.ServiceDefaults;
using Mediator;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();
builder.AddServiceDefaults();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddApi();
builder.Services.AddMediator((MediatorOptions options) =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.Assemblies = [typeof(CreateUserCommand)];
    options.PipelineBehaviors =
    [
        typeof(RequestDiagnosticsBehavior<,>),
        typeof(AuthorizationBehavior<,>),
        typeof(ValidationBehavior<,>)
    ];
});

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    if (string.IsNullOrWhiteSpace(app.Configuration["Jwt:Key"]))
        app.Logger.LogWarning("Using a temporary development signing key. Tokens expire when the application restarts.");

    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    app.MapOpenApi();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapUserEndpoints();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
app.Run();

public partial class Program;
