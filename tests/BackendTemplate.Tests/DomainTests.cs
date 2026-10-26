using BackendTemplate.Domain.Entities.Users;
using BackendTemplate.Domain.Events.Users;
using BackendTemplate.Domain.ValueObjects;
using BackendTemplate.Infrastructure.Authentication;

namespace BackendTemplate.Tests;

public sealed class DomainTests
{
    [Theory]
    [InlineData(" Alice@Example.COM ", "alice@example.com")]
    [InlineData("user+tag@example.com", "user+tag@example.com")]
    public void EmailIsNormalized(string input, string expected)
    {
        var result = Email.Create(input);
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("bad")]
    [InlineData("Alice <alice@example.com>")]
    public void InvalidEmailIsRejected(string input) => Assert.True(Email.Create(input).IsFailure);

    [Fact]
    public void FailedResultsSupportValueTypes()
    {
        var result = Result<int>.Failure("Not found");
        Assert.True(result.IsFailure);
        Assert.Equal("Not found", result.Error);
    }

    [Fact]
    public void PasswordHashesAreSaltedAndMalformedHashesAreRejected()
    {
        var hasher = new PasswordHasher();
        var first = hasher.HashPassword("a long password");
        Assert.NotEqual(first, hasher.HashPassword("a long password"));
        Assert.True(hasher.VerifyPassword(first, "a long password"));
        Assert.False(hasher.VerifyPassword(first, "wrong password"));
        Assert.False(hasher.VerifyPassword("invalid hash", "a long password"));
    }

    [Fact]
    public void RegistrationRaisesEventWithAssignedIdAndNoCredentials()
    {
        var user = User.Create("Alice", "Example", (Email)"alice@example.com", "test-hash", DateTimeOffset.UtcNow);
        var created = Assert.IsType<UserCreatedEvent>(Assert.Single(user.DomainEvents));
        Assert.Equal(user.Id, created.UserId);
        Assert.NotEqual(Guid.Empty, created.UserId);
        Assert.Equal("alice@example.com", created.Email);
    }

    [Fact]
    public void FailedAttemptsResetAfterLockoutExpires()
    {
        var now = DateTimeOffset.UtcNow;
        var user = User.Create("Alice", "Example", (Email)"alice@example.com", "test-hash", now);
        for (var i = 0; i < 5; i++) user.RecordFailedLogin(now);
        user.RecordFailedLogin(now.AddMinutes(16));
        Assert.Equal(1, user.LoginFailedCount);
        Assert.False(user.IsLockedOut(now.AddMinutes(16)));
    }
}
