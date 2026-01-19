using Algocura.Platform.Messaging.Publishing;

namespace Algocura.Reporting.Api.Messaging;

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
    : IntegrationEvent(
        EventId,
        OccurredAtUtc)
{
    public override string EventType =>
        nameof(ReportingSnapshot);
}

public interface IEventPublisher
{
    Task PublishAsync(
        IntegrationEvent integrationEvent,
        string? correlationId = null,
        CancellationToken cancellationToken = default);
}

public sealed class ServiceBusEventPublisher(
    IPlatformMessagePublisher publisher,
    ILogger<ServiceBusEventPublisher> logger)
    : IEventPublisher
{
    public async Task PublishAsync(
        IntegrationEvent integrationEvent,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            integrationEvent);

        var applicationProperties =
            new Dictionary<string, object>
            {
                ["occurredAtUtc"] =
                    integrationEvent.OccurredAtUtc.ToString("O")
            };

        await publisher.PublishAsync(
            integrationEvent,
            integrationEvent.EventType,
            integrationEvent.EventId,
            correlationId,
            applicationProperties,
            cancellationToken);

        logger.LogInformation(
            "Published {EventType} with ID {EventId}.",
            integrationEvent.EventType,
            integrationEvent.EventId);
    }
}