namespace Algocura.Audit.Api.Messaging;

public sealed class ServiceBusOptions
{
    public const string SectionName =
        "ServiceBus";

    public bool UseEmulator { get; set; }

    public string ConnectionString { get; set; } =
        string.Empty;

    public string FullyQualifiedNamespace { get; set; } =
        string.Empty;

    public string TopicName { get; set; } =
        "algocura-events";

    public string SubscriptionName { get; set; } =
        "audit-api";
}
