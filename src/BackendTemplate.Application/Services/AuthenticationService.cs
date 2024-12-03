using System.Security.Cryptography;
using System.Text;

using BackendTemplate.Application.Exceptions;
using BackendTemplate.Application.Interface.Repositories.Queries;
using BackendTemplate.Application.Interface.Services;

namespace BackendTemplate.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserQueryRepository _userQueryRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthenticationService(
        IUserQueryRepository userQueryRepository,
        IJwtService jwtService,
        IPasswordHasher passwordHasher
    )
    {
        _userQueryRepository = userQueryRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }

    public async Task<string> AuthenticateAsync(string email, string password)
    {
        var user = await _userQueryRepository.FirstOrDefaultAsync(user => user.Email.Equals(email));

        if (
            user == null
            || !_passwordHasher.VerifyHashedPassword(password, user.PasswordHash, user.PasswordSalt)
        )
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        return _jwtService.GenerateToken(user);
    }
}
