namespace Algocura.CompoundCatalog.Api.Messaging;

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    public bool UseEmulator { get; init; }

    public string ConnectionString { get; init; } = string.Empty;

    public string FullyQualifiedNamespace { get; init; } = string.Empty;

    public string TopicName { get; init; } = string.Empty;
}