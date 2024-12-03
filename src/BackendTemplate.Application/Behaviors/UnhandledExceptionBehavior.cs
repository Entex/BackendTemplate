using MediatR;
using Microsoft.Extensions.Logging;
using BackendTemplate.Application.Exceptions;

namespace BackendTemplate.Application.Behaviors;

public class UnhandledExceptionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

    public UnhandledExceptionBehavior(
        ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger
    )
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await next(); // Proceed to the next behavior or handler
        }
        catch (HttpException ex)
        {
            // Log specific HttpException details
            _logger.LogError(
                ex,
                "HTTP exception for request: {RequestName}. Status code: {StatusCode}, Request data: {@Request}",
                typeof(TRequest).Name,
                ex.StatusCode,
                request
            );

            throw; // Re-throw to propagate to higher layers (e.g., middleware)
        }
        catch (Exception ex)
        {
            // Log generic exceptions
            _logger.LogError(
                ex,
                "Unhandled exception for request: {RequestName}. Request data: {@Request}",
                typeof(TRequest).Name,
                request
            );

            throw; // Re-throw to propagate to higher layers
        }
    }
}
