using MediatR;
using Microsoft.Extensions.Logging;

namespace BackendTemplate.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        // Log request details
        _logger.LogInformation(
            "Handling request: {RequestName} with data: {@Request}",
            typeof(TRequest).Name,
            request
        );

        var response = await next(); // Proceed to the next behavior or handler

        // Log response details
        _logger.LogInformation(
            "Handled request: {RequestName} with response: {@Response}",
            typeof(TRequest).Name,
            response
        );

        return response;
    }
}
