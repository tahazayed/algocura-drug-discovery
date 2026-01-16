namespace Algocura.Platform.Auditing.Options;

public sealed class AuditingOptions
{
    public const string SectionName = "Auditing";

    public string ServiceName { get; set; } = string.Empty;

    public bool PublishEntityAuditEvents { get; set; } = true;

    public bool StampCreatedAndUpdatedFields { get; set; } = true;

    public bool IncludeUnchangedPropertiesOnCreate { get; set; } = true;

    public bool IncludeUnchangedPropertiesOnDelete { get; set; } = true;

    public bool IgnoreAuditInfrastructureEntities { get; set; } = true;

    public HashSet<string> ExcludedEntityTypes { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

    public HashSet<string> ExcludedPropertyNames { get; set; } =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Password",
            "PasswordHash",
            "AccessToken",
            "RefreshToken",
            "ClientSecret",
            "ConnectionString",
            "SharedAccessKey",
            "ApiKey"
        };

    public HashSet<string> MaskedPropertyNames { get; set; } =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Email",
            "Recipient",
            "IpAddress",
            "UserAgent"
        };
}
