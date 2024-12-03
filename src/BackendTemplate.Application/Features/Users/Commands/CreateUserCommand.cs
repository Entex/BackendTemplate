using AutoMapper;

using BackendTemplate.Application.Interface.Repositories.Commands;
using BackendTemplate.Application.Interface.Services;
using BackendTemplate.Application.Requirements.MustHavePermission;
using BackendTemplate.Application.Requirements.Providers;
using BackendTemplate.Domain.Entities.Users;

using MediatR;

using Microsoft.AspNetCore.Authorization;

namespace BackendTemplate.Application.Features.Users.Commands;

public record CreateUserCommand(string FirstName, string LastName, string Email, string Password)
    : IRequest<Guid>,
        IAuthorizationRequirementProvider
{
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        return [new MustHavePermissionRequirement("Create User")];
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IMapper _mapper;
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(IMapper mapper, IUserCommandRepository userCommandRepository, IPasswordHasher passwordHasher)
    {
        _mapper = mapper;
        _userCommandRepository = userCommandRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {

        var user = _mapper.Map<User>(request);
        user.PasswordHash = _passwordHasher.HashPassword(request.Password, out var passwordSalt);
        user.PasswordSalt = passwordSalt;

        user = await _userCommandRepository.AddAsync(user);

        return user.Id;
    }
}
