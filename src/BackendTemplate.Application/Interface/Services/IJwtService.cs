using BackendTemplate.Domain.Entities.Users;

namespace BackendTemplate.Application.Interface.Services;

public interface IJwtService
{
    string GenerateToken(User user, bool rememberMe = false);
}
