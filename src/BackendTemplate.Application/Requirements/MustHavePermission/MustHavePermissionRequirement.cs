using Microsoft.AspNetCore.Authorization;

namespace BackendTemplate.Application.Requirements.MustHavePermission;

public sealed record MustHavePermissionRequirement(string Permission, Guid? OwnerId = null) : IAuthorizationRequirement;
