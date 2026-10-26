using System.Diagnostics;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BackendTemplate.Application.Behaviors;

public sealed class RequestDiagnosticsBehavior<TRequest, TResponse>(ILogger<RequestDiagnosticsBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        using var activity = ApplicationDiagnostics.ActivitySource.StartActivity(name);
        var startedAt = Stopwatch.GetTimestamp();
        var status = "success";
        try
        {
            return await next(request, cancellationToken);
        }
        catch (Exception exception)
        {
            status = exception is OperationCanceledException ? "cancelled" : "failure";
            activity?.SetStatus(ActivityStatusCode.Error);
            throw;
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
            ApplicationDiagnostics.RequestDuration.Record(elapsed,
                new KeyValuePair<string, object?>("request.name", name),
                new KeyValuePair<string, object?>("request.status", status));
            // Request bodies and responses can contain passwords or tokens.
            logger.LogInformation("Request {RequestName} finished with {Status} in {ElapsedMs:F2} ms",
                name, status, elapsed);
        }
    }
}
