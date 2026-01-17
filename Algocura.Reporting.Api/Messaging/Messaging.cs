using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.Reporting.Api.Messaging;

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";
    public bool UseEmulator { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public string FullyQualifiedNamespace { get; set; } = string.Empty;
    public string TopicName { get; set; } = "algocura-events";
    public string SubscriptionName { get; set; } = "reporting-api";
}

public abstract record IntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc)
{
    public abstract string EventType { get; }
}

public sealed record ReportingSnapshot(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    int ProjectionCount,
    int ActiveReportDefinitions,
    int CompletedReportRuns)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType => nameof(ReportingSnapshot);
}

public interface IEventPublisher
{
    Task PublishAsync(
        IntegrationEvent integrationEvent,
        string? correlationId = null,
        CancellationToken cancellationToken = default);
}

public sealed class ServiceBusEventPublisher(
    ServiceBusClient client,
    IOptions<ServiceBusOptions> options)
    : IEventPublisher,
      IAsyncDisposable
{
    private readonly ServiceBusSender _sender =
        client.CreateSender(options.Value.TopicName);

    public async Task PublishAsync(
        IntegrationEvent integrationEvent,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var message = new ServiceBusMessage(
            BinaryData.FromString(
                JsonSerializer.Serialize(
                    integrationEvent,
                    integrationEvent.GetType())))
        {
            MessageId = integrationEvent.EventId.ToString(),
            Subject = integrationEvent.EventType,
            CorrelationId = correlationId,
            ContentType = "application/json"
        };

        message.ApplicationProperties["eventType"] =
            integrationEvent.EventType;

        await _sender.SendMessageAsync(
            message,
            cancellationToken);
    }

    public ValueTask DisposeAsync() =>
        _sender.DisposeAsync();
}
