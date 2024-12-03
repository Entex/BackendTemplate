using Microsoft.AspNetCore.Authorization;

namespace BackendTemplate.Application.Requirements.MustHavePermission;

public class MustHavePermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public MustHavePermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
