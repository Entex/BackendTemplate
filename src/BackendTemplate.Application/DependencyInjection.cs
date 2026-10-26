using BackendTemplate.Application.Events;
using BackendTemplate.Application.Features.Users.EventHandlers;
using BackendTemplate.Application.Requirements;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BackendTemplate.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddHttpContextAccessor();
        services.AddAuthorization();
        services.AddScoped<IAuthorizationHandler, MustHavePermissionHandler>();
        services.AddScoped<IDomainEventHandler, UserCreatedEventHandler>();
        services.AddSingleton(TimeProvider.System);
        return services;
    }
}
