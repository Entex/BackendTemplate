using MediatR;
using System.Diagnostics.Metrics;

namespace BackendTemplate.Application.Behaviors;

public class MetricBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly Meter Meter = new("BackendTemplate.Application", "1.0");
    private static readonly Counter<long> RequestCounter = Meter.CreateCounter<long>(
        "requests_total",
        "Requests",
        "Counts total requests processed"
    );
    private static readonly Histogram<double> RequestDuration = Meter.CreateHistogram<double>(
        "request_duration_ms",
        "Milliseconds",
        "Measures request processing time"
    );

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var response = await next();

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Record metrics
            RequestCounter.Add(
                1,
                new KeyValuePair<string, object?>("request_name", typeof(TRequest).Name),
                new KeyValuePair<string, object?>("status", "success")
            );
            RequestDuration.Record(
                duration,
                new KeyValuePair<string, object?>("request_name", typeof(TRequest).Name),
                new KeyValuePair<string, object?>("status", "success")
            );

            return response;
        }
        catch (Exception)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Record failed request metrics
            RequestCounter.Add(
                1,
                new KeyValuePair<string, object?>("request_name", typeof(TRequest).Name),
                new KeyValuePair<string, object?>("status", "failure")
            );
            RequestDuration.Record(
                duration,
                new KeyValuePair<string, object?>("request_name", typeof(TRequest).Name),
                new KeyValuePair<string, object?>("status", "failure")
            );

            throw; // Re-throw the exception
        }
    }
}
