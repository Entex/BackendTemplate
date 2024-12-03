using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BackendTemplate.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddMediatR(
            options => options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
        );

        return services;
    }
}
