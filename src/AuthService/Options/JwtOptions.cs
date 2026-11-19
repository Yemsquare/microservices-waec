namespace AuthService.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Key { get; init; } = "";
    public string Issuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public int AccessTokenMinutes { get; init; } = 10;
    public int RefreshTokenDays { get; init; } = 7;
}

