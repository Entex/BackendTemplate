namespace BackendTemplate.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "BackendTemplate";
    public string Audience { get; set; } = "BackendTemplate.Client";
    public int LifetimeMinutes { get; set; } = 60;
}
