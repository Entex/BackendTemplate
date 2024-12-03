using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using BackendTemplate.Application.Configurations;
using BackendTemplate.Application.Interface.Services;
using BackendTemplate.Domain.Entities.Users;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BackendTemplate.Application.Services;

public class JwtService : IJwtService
{
    private readonly JwtConfiguration _jwtConfiguration;

    public JwtService(JwtConfiguration jwtConfiguration)
    {
        _jwtConfiguration = jwtConfiguration;
    }

    public string GenerateToken(User user, bool rememberMe = false)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Construct claims in one go
        var roleClaims = user.Roles.Select(role => new Claim(ClaimTypes.Role, role.Name));
        var permissionClaims = user.Roles
            .SelectMany(role => role.Permissions.Select(permission => permission.Name))
            .Concat(user.Permissions.Select(permission => permission.Name))
            .Distinct()
            .Select(permission => new Claim("permission", permission));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FirstName),
            new(ClaimTypes.Surname, user.LastName)
        }
            .Concat(roleClaims)
            .Concat(permissionClaims)
            .ToList();

        var token = new JwtSecurityToken(
            issuer: _jwtConfiguration.Issuer,
            audience: _jwtConfiguration.Audience,
            claims: claims,
            expires: rememberMe ? DateTime.UtcNow.AddMonths(1) : DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
