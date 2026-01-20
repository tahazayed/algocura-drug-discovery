namespace Algocura.Platform.Messaging.Options;

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

    // Optional at the platform-messaging level because publisher-only
    // services do not require a subscription.
    public string? SubscriptionName { get; set; }

    public int DefaultMessageTimeToLiveDays { get; set; } =
        7;

    public int MaxConcurrentCalls { get; set; } =
        4;

    public int PrefetchCount { get; set; } =
        0;

    public int MaxAutoLockRenewalMinutes { get; set; } =
        5;
}
