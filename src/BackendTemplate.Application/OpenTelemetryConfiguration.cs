using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BackendTemplate.Application;

public static class OpenTelemetryConfiguration
{
    public static IServiceCollection AddOpenTelemetryServices(this IServiceCollection services)
    {
        // Define the OpenTelemetry ResourceBuilder
        var resourceBuilder = ResourceBuilder.CreateDefault().AddService("BackendTemplate");

        services
            .AddOpenTelemetry()
            .WithTracing(
                traceBuilder =>
                    traceBuilder
                        .SetResourceBuilder(resourceBuilder) // Shared resource configuration
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddSqlClientInstrumentation()
                        .AddConsoleExporter()
            )
            .WithMetrics(
                metricBuilder =>
                    metricBuilder
                        .SetResourceBuilder(resourceBuilder) // Shared resource configuration
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddConsoleExporter()
            )
            .WithLogging(loggingBuilder => loggingBuilder.AddConsoleExporter());

        return services;
    }
}
