using System.Security.Cryptography;

using BackendTemplate.Application.Interface.Services;

namespace BackendTemplate.Application.Services;

public class PasswordHasher : IPasswordHasher
{
    public byte[] HashPassword(string password, out byte[] salt)
    {
        var rng = RandomNumberGenerator.Create();
        salt = new byte[256];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA512);
        return pbkdf2.GetBytes(512);
    }

    public bool VerifyHashedPassword(string password, byte[] hashedPassword, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA512);
        var hashedPasswordToVerify = pbkdf2.GetBytes(512);

        return hashedPasswordToVerify.SequenceEqual(hashedPassword);
    }
}
