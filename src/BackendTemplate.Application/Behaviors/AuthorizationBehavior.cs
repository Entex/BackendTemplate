using BackendTemplate.Application.Exceptions;
using BackendTemplate.Application.Requirements.Providers;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace BackendTemplate.Application.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse>(
    IAuthorizationService authorization,
    IHttpContextAccessor httpContextAccessor)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IAuthorizationRequirementProvider provider)
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                throw new UnauthorizedException();

            var result = await authorization.AuthorizeAsync(user, request, provider.GetRequirements());
            if (!result.Succeeded)
                throw new ForbiddenException();
        }

        return await next(request, cancellationToken);
    }
}
