namespace BackendTemplate.Application.Interface.Services;

public interface IAuthenticationService
{
    Task<string> AuthenticateAsync(string email, string password);
}
