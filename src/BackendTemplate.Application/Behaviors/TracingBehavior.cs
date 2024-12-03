using MediatR;
using System.Diagnostics;

namespace BackendTemplate.Application.Behaviors;

public class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var activityName = $"{typeof(TRequest).Name}";

        using var activity = new Activity(activityName);
        activity.Start();

        try
        {
            activity.SetTag("RequestType", typeof(TRequest).Name);
            activity.SetTag("ResponseType", typeof(TResponse).Name);

            var response = await next(); // Proceed to the next behavior or handler

            activity.SetStatus(ActivityStatusCode.Ok);
            activity.SetTag("RequestStatus", "Success");

            return response;
        }
        catch (Exception ex)
        {
            activity.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity.SetTag("RequestStatus", "Failure");
            activity.SetTag("ExceptionType", ex.GetType().Name);
            activity.SetTag("ExceptionMessage", ex.Message);

            throw; // Re-throw the exception to propagate it
        }
        finally
        {
            activity.Stop();
        }
    }
}
