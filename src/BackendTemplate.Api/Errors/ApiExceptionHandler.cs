using BackendTemplate.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BackendTemplate.Api.Errors;

public sealed class ApiExceptionHandler(IProblemDetailsService problemDetails, ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && cancellationToken.IsCancellationRequested)
        {
            httpContext.Response.StatusCode = 499;
            return true;
        }

        var statusCode = exception switch
        {
            HttpException httpException => httpException.StatusCode,
            BadHttpRequestException badRequest => badRequest.StatusCode,
            _ => StatusCodes.Status500InternalServerError
        };
        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled error for {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);

        ProblemDetails problem = exception is ValidationException { Errors: not null } validation
            ? new HttpValidationProblemDetails(validation.Errors.ToDictionary(x => x.Key, x => x.Value.ToArray()))
            : new ProblemDetails();
        problem.Status = statusCode;
        problem.Title = statusCode switch
        {
            400 => "Invalid request.",
            401 => "Authentication required.",
            403 => "Access denied.",
            404 => "Resource not found.",
            409 => "Conflict.",
            _ => "An unexpected error occurred."
        };
        problem.Detail = exception is HttpException ? exception.Message : null;
        httpContext.Response.StatusCode = statusCode;
        await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem
        });
        return true;
    }
}
