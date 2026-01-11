namespace Algocura.Screening.Api.Events;

public sealed record CompletedScreeningSnapshot : IntegrationEvent
{
    public CompletedScreeningSnapshot(
        Guid eventId,
        DateTimeOffset occurredAtUtc,
        Guid screeningId,
        string name,
        string screeningMethod,
        DateTimeOffset completedAtUtc,
        IReadOnlyCollection<CompletedScreeningSnapshotResult> results)
        : base(eventId, occurredAtUtc)
    {
        ScreeningId = screeningId;
        Name = name;
        ScreeningMethod = screeningMethod;
        CompletedAtUtc = completedAtUtc;
        Results = results;
    }

    public override string EventType =>
        nameof(CompletedScreeningSnapshot);

    public Guid ScreeningId { get; init; }

    public string Name { get; init; }

    public string ScreeningMethod { get; init; }

    public DateTimeOffset CompletedAtUtc { get; init; }

    public IReadOnlyCollection<CompletedScreeningSnapshotResult>
        Results
    { get; init; }
}

public sealed record CompletedScreeningSnapshotResult(
    Guid CompoundId,
    decimal Score,
    int Rank,
    bool Selected);