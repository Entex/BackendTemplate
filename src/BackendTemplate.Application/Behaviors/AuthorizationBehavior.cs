using BackendTemplate.Application.Exceptions;
using BackendTemplate.Application.Requirements.Providers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace BackendTemplate.Application.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationBehavior(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (request is IAuthorizationRequirementProvider authorizationProvider)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                throw new InvalidOperationException(
                    "No HttpContext is available for authorization."
                );
            }

            var user = httpContext.User;
            var requirements = authorizationProvider.GetRequirements();

            // Authorize with all requirements at once
            var result = await _authorizationService.AuthorizeAsync(user, null, requirements);

            if (!result.Succeeded)
            {
                throw new UnauthorizedException("User is not authorized to perform this action.");
            }
        }

        return await next();
    }
}
