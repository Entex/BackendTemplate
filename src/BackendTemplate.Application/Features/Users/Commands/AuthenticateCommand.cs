using BackendTemplate.Application.Abstractions;
using BackendTemplate.Application.Exceptions;
using BackendTemplate.Domain.ValueObjects;
using Mediator;

namespace BackendTemplate.Application.Features.Users.Commands;

public sealed record AuthenticateCommand(string Email, string Password) : IRequest<AuthenticationResponse>;

public sealed class AuthenticateCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ITokenService tokens,
    TimeProvider timeProvider) : IRequestHandler<AuthenticateCommand, AuthenticationResponse>
{
    public async ValueTask<AuthenticationResponse> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var user = email.IsSuccess
            ? await users.GetByEmailAsync(email.Value!.Value, cancellationToken)
            : null;
        var now = timeProvider.GetUtcNow();
        var passwordMatches = passwordHasher.VerifyPassword(user?.PasswordHash, request.Password);

        if (user is null || user.IsLockedOut(now) || user.IsPasswordExpired)
            throw new UnauthorizedException("Invalid email or password.");

        if (!passwordMatches)
        {
            user.RecordFailedLogin(now);
            await users.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Invalid email or password.");
        }

        user.RecordSuccessfulLogin(now);
        await users.SaveChangesAsync(cancellationToken);
        return tokens.CreateToken(user);
    }
}
