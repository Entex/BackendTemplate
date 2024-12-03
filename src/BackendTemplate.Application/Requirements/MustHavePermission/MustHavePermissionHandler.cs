using BackendTemplate.Application.Interface.Repositories.Queries;
using BackendTemplate.Application.Requirements.MustHavePermission;

using Microsoft.AspNetCore.Authorization;

using System.Security.Claims;

namespace BackendTemplate.Application.Requirements;

public class MustHavePermissionHandler : AuthorizationHandler<MustHavePermissionRequirement>
{
    private readonly IPermissionQueryRepository _permissionQueryRepository;

    public MustHavePermissionHandler(IPermissionQueryRepository permissionQueryRepository)
    {
        _permissionQueryRepository = permissionQueryRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MustHavePermissionRequirement requirement
    )
    {
        // Step 1: Check claims for the required permission
        var userPermissions = context.User.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value);

        if (userPermissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }

        // Step 2: Check the database for the required permission
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (
            Guid.TryParse(userId, out var parsedUserId)
            && await _permissionQueryRepository.HasPermissionAsync(
                parsedUserId,
                requirement.Permission
            )
        )
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail(); // User lacks the required permission
    }
}
