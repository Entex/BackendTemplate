namespace BackendTemplate.Application.Configurations;

public class JwtConfiguration
{
    public required string Key { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
}