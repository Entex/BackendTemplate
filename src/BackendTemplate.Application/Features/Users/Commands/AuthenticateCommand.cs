using BackendTemplate.Application.Interface.Repositories.Queries;
using BackendTemplate.Application.Interface.Services;

using MediatR;

namespace BackendTemplate.Application.Features.Users.Commands;

public record AuthenticateCommand(string Email, string Password) : IRequest<string>;

public class AuthenticateCommandHandler : IRequestHandler<AuthenticateCommand, string>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IUserQueryRepository _userQueryRepository;

    public AuthenticateCommandHandler(
        IAuthenticationService authenticationService,
        IUserQueryRepository userQueryRepository
    )
    {
        _authenticationService = authenticationService;
        _userQueryRepository = userQueryRepository;
    }

    public async Task<string> Handle(
        AuthenticateCommand request,
        CancellationToken cancellationToken
    )
    {
        var token = await _authenticationService.AuthenticateAsync(request.Email, request.Password);
        return token;
    }
}
