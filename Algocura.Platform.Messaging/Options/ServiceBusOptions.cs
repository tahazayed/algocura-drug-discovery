namespace Algocura.Platform.Messaging.Options;

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    public bool UseEmulator { get; set; }

    public string ConnectionString { get; set; } =
        string.Empty;

    public string FullyQualifiedNamespace { get; set; } =
        string.Empty;

    public string TopicName { get; set; } =
        "algocura-events";
}
