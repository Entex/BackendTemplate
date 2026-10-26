using BackendTemplate.Domain.Entities.Users;

namespace BackendTemplate.Application.Features.Users;

public sealed record UserResponse(Guid Id, string FirstName, string LastName, string Email, DateTimeOffset CreatedAt)
{
    public static UserResponse FromUser(User user) =>
        new(user.Id, user.FirstName, user.LastName, user.Email.Value, user.CreatedAt);
}

public sealed record AuthenticationResponse(string AccessToken, DateTimeOffset ExpiresAt, string TokenType = "Bearer");
