namespace Algocura.AIDiscovery.Api.Events;

public sealed record CompletedScreeningSnapshot
{
    public Guid EventId { get; init; }

    public DateTimeOffset OccurredAtUtc { get; init; }

    public Guid ScreeningId { get; init; }

    public required string Name { get; init; }

    public required string ScreeningMethod { get; init; }

    public DateTimeOffset CompletedAtUtc { get; init; }

    public IReadOnlyCollection<CompletedScreeningSnapshotResult>
        Results
    { get; init; } = [];
}

public sealed record CompletedScreeningSnapshotResult
{
    public Guid CompoundId { get; init; }

    public decimal Score { get; init; }

    public int Rank { get; init; }

    public bool Selected { get; init; }
}
