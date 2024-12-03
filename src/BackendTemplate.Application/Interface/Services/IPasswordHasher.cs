namespace BackendTemplate.Application.Interface.Services;

public interface IPasswordHasher
{
    byte[] HashPassword(string password, out byte[] salt);
    bool VerifyHashedPassword(string password, byte[] hashedPassword, byte[] salt);
}
