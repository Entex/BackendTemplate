using BackendTemplate.Application.Abstractions;
using BackendTemplate.Application.Exceptions;
using BackendTemplate.Domain.Entities.Users;
using BackendTemplate.Domain.ValueObjects;
using Mediator;

namespace BackendTemplate.Application.Features.Users.Commands;

// Public registration deliberately accepts no roles, permissions, or administrator flag.
public sealed record CreateUserCommand(string FirstName, string LastName, string Email, string Password)
    : IRequest<UserResponse>;

public sealed class CreateUserCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider) : IRequestHandler<CreateUserCommand, UserResponse>
{
    public async ValueTask<UserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            throw new ValidationException(emailResult.Error);

        var email = emailResult.Value!;
        if (await users.EmailExistsAsync(email.Value, cancellationToken))
            throw new ConflictException("A user with this email already exists.");

        var user = User.Create(request.FirstName, request.LastName, email,
            passwordHasher.HashPassword(request.Password), timeProvider.GetUtcNow());
        await users.AddAsync(user, cancellationToken);
        return UserResponse.FromUser(user);
    }
}
