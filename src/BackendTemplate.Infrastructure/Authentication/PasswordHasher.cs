using BackendTemplate.Application.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BackendTemplate.Infrastructure.Authentication;

public sealed class PasswordHasher : IPasswordHasher
{
    private static readonly object User = new();
    private readonly PasswordHasher<object> _hasher = new(
        Options.Create(new PasswordHasherOptions { IterationCount = 210_000 }));
    private readonly string _dummyHash;

    public PasswordHasher()
    {
        _dummyHash = _hasher.HashPassword(User, Guid.NewGuid().ToString());
    }

    public string HashPassword(string password) => _hasher.HashPassword(User, password);

    public bool VerifyPassword(string? passwordHash, string password)
    {
        try
        {
            // Unknown accounts still perform the expensive password check.
            var result = _hasher.VerifyHashedPassword(User, passwordHash ?? _dummyHash, password);
            return passwordHash is not null && result != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
