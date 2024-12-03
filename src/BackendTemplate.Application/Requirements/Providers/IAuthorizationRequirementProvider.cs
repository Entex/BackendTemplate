using Microsoft.AspNetCore.Authorization;

namespace BackendTemplate.Application.Requirements.Providers;

public interface IAuthorizationRequirementProvider
{
    IEnumerable<IAuthorizationRequirement> GetRequirements();
}
