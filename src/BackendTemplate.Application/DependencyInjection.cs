using BackendTemplate.Application.Behaviors;
using BackendTemplate.Application.Interface.Services;
using BackendTemplate.Application.Requirements;
using BackendTemplate.Application.Requirements.MustHavePermission;
using BackendTemplate.Application.Services;

using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BackendTemplate.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Add MediatR
        services.AddMediatR(
            cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly)
        );

        // Add Validators
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Add AutoMapper
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        // Add External Services
        services.AddOpenTelemetryServices();
        services.AddAuthenticationServices(configuration);

        // Add pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MetricBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));

        // Authorization requirements
        services.AddTransient<IAuthorizationRequirement, MustHavePermissionRequirement>();

        // Authorization handlers
        services.AddSingleton<IAuthorizationHandler, MustHavePermissionHandler>();

        // Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }
}
