namespace Algocura.Reporting.Api.Messaging;

public abstract record IntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc)
    : Algocura.Platform.Messaging.Events.IntegrationEvent(
        EventId,
        OccurredAtUtc)
{
    public override string Source =>
        "Algocura.Reporting.Api";
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
