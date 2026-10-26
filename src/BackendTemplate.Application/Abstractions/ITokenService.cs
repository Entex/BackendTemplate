using BackendTemplate.Application.Features.Users;
using BackendTemplate.Domain.Entities.Users;

namespace BackendTemplate.Application.Abstractions;

public interface ITokenService
{
    AuthenticationResponse CreateToken(User user);
}
