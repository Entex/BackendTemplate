using System.Security.Claims;
using BackendTemplate.Application.Abstractions;
using BackendTemplate.Application.Requirements.MustHavePermission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace BackendTemplate.Application.Requirements;

public sealed class MustHavePermissionHandler(IUserRepository users, IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<MustHavePermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, MustHavePermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true
            || !Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return;

        if (requirement.OwnerId == userId
            || await users.HasPermissionAsync(userId, requirement.Permission,
                httpContextAccessor.HttpContext?.RequestAborted ?? default))
            context.Succeed(requirement);
    }
}
