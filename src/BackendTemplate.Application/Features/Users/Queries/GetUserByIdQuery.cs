using BackendTemplate.Application.Abstractions;
using BackendTemplate.Application.Exceptions;
using BackendTemplate.Application.Requirements.MustHavePermission;
using BackendTemplate.Application.Requirements.Providers;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace BackendTemplate.Application.Features.Users.Queries;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<UserResponse>, IAuthorizationRequirementProvider
{
    public IEnumerable<IAuthorizationRequirement> GetRequirements() =>
        [new MustHavePermissionRequirement("users:read", Id)];
}

public sealed class GetUserByIdQueryHandler(IUserRepository users) : IRequestHandler<GetUserByIdQuery, UserResponse>
{
    public async ValueTask<UserResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("User", request.Id);
        return UserResponse.FromUser(user);
    }
}
