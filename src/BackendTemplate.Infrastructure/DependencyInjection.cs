using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BackendTemplate.Application.Abstractions;
using BackendTemplate.Infrastructure.Authentication;
using BackendTemplate.Infrastructure.Persistence;
using BackendTemplate.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BackendTemplate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        switch (configuration["Database:Provider"]?.ToLowerInvariant())
        {
            case "sqlite":
                services.AddDbContext<SqliteDbContext>((provider, options) => options.UseSqlite(
                    GetConnectionString(provider)));
                services.AddScoped<AppDbContext>(provider => provider.GetRequiredService<SqliteDbContext>());
                break;
            case "sqlserver":
                services.AddDbContext<SqlServerDbContext>((provider, options) => options.UseSqlServer(GetConnectionString(provider),
                    sql => sql.EnableRetryOnFailure()));
                services.AddScoped<AppDbContext>(provider => provider.GetRequiredService<SqlServerDbContext>());
                break;
            default:
                throw new InvalidOperationException("Database:Provider must be Sqlite or SqlServer.");
        }

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .PostConfigure(options =>
            {
                if (environment.IsDevelopment() && string.IsNullOrWhiteSpace(options.Key))
                    options.Key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            })
            .Validate(options => !string.IsNullOrWhiteSpace(options.Key)
                && Encoding.UTF8.GetByteCount(options.Key) >= 32, "Jwt:Key must contain at least 32 UTF-8 bytes.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience is required.")
            .Validate(options => options.LifetimeMinutes is >= 1 and <= 120, "Jwt:LifetimeMinutes must be between 1 and 120.")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, options) =>
            {
                var jwt = options.Value;
                bearer.MapInboundClaims = true;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ClockSkew = TimeSpan.FromSeconds(15)
                };
                bearer.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        if (!Guid.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
                        {
                            context.Fail("Invalid subject.");
                            return;
                        }

                        var user = await context.HttpContext.RequestServices.GetRequiredService<IUserRepository>()
                            .GetByIdAsync(id, context.HttpContext.RequestAborted);
                        var now = context.HttpContext.RequestServices.GetRequiredService<TimeProvider>().GetUtcNow();
                        if (user is null || user.IsLockedOut(now) || user.IsPasswordExpired)
                            context.Fail("Account is unavailable.");
                    }
                };
            });
        return services;
    }

    private static string GetConnectionString(IServiceProvider provider) =>
        provider.GetRequiredService<IConfiguration>().GetConnectionString("Database")
        ?? throw new InvalidOperationException("ConnectionStrings:Database must be configured.");
}
