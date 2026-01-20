namespace Algocura.Identity.Api.Security;

public sealed class JwtOptions
{
    public const string SectionName =
        "Jwt";

    public string Issuer { get; set; } =
        "Algocura.Identity.Api";

    public string Audience { get; set; } =
        "Algocura.Platform";

    public string SigningKey { get; set; } =
        string.Empty;

    public int AccessTokenMinutes { get; set; } =
        60;
}
