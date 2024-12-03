using System.Reflection;

using BackendTemplate.Application.Interface.Repositories.Commands;
using BackendTemplate.Application.Interface.Repositories.Queries;
using BackendTemplate.Application.Interface.Services;
using BackendTemplate.Infrastructure.Repositories;
using BackendTemplate.Infrastructure.Repositories.Commands;
using BackendTemplate.Infrastructure.Repositories.Queries;
using BackendTemplate.Infrastructure.Services;

using Microsoft.Extensions.DependencyInjection;

namespace BackendTemplate.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {

        // Command Repositories
        services.AddScoped<IUserCommandRepository, UserCommandRepository>();
        services.AddScoped<IRoleCommandRepository, RoleCommandRepository>();
        services.AddScoped<IPermissionCommandRepository, PermissionCommandRepository>();

        // Query Repositories
        services.AddScoped<IUserQueryRepository, UserQueryRepository>();
        services.AddScoped<IRoleQueryRepository, RoleQueryRepository>();
        services.AddScoped<IPermissionQueryRepository, PermissionQueryRepository>();

        // Services
        services.AddScoped<IEmailService, MockEmailService>();

        return services;
    }
}
