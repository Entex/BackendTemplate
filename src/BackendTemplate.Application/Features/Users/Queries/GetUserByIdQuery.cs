using BackendTemplate.Application.Interface.Repositories.Queries;
using BackendTemplate.Application.Requirements.MustHavePermission;
using BackendTemplate.Application.Requirements.Providers;
using BackendTemplate.Domain.Entities.Users;

using MediatR;

using Microsoft.AspNetCore.Authorization;

namespace BackendTemplate.Application.Features.Users.Queries;

public record GetUserByIdQuery(Guid Id) : IRequest<User?>, IAuthorizationRequirementProvider
{
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        return
        [
            new MustHavePermissionRequirement("View User")
        ];
    }
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, User?>
{
    private readonly IUserQueryRepository _userQueryRepository;

    public GetUserByIdQueryHandler(IUserQueryRepository userQueryRepository)
    {
        _userQueryRepository = userQueryRepository;
    }

    public async Task<User?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return await _userQueryRepository.GetByIdAsync(request.Id);
    }
}