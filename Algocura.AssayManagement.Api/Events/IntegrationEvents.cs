using Algocura.Platform.Messaging.Events;

namespace Algocura.AssayManagement.Api.Events;

public abstract record AssayManagementIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc)
    : IntegrationEvent(
        EventId,
        OccurredAtUtc)
{
    public override string Source =>
        "Algocura.AssayManagement.Api";
}

// Consumed from Screening API.
public sealed record CompletedScreeningSnapshot
{
    public Guid EventId { get; init; }

    public DateTimeOffset OccurredAtUtc { get; init; }

    public Guid ScreeningId { get; init; }

    public required string Name { get; init; }

    public required string ScreeningMethod { get; init; }

    public DateTimeOffset CompletedAtUtc { get; init; }

    public IReadOnlyCollection<CompletedScreeningSnapshotResult> Results { get; init; } = [];
}

public sealed record CompletedScreeningSnapshotResult
{
    public Guid CompoundId { get; init; }

    public decimal Score { get; init; }

    public int Rank { get; init; }

    public bool Selected { get; init; }
}

public sealed record AssayCreated(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid AssayId,
    Guid ScreeningId,
    string Name,
    string AssayType)
    : AssayManagementIntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType =>
        nameof(AssayCreated);
}

public sealed record AssayCompleted(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid AssayId,
    Guid ScreeningId,
    int ResultCount,
    string QualityControlStatus)
    : AssayManagementIntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType =>
        nameof(AssayCompleted);
}

public sealed record AssaySnapshot(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid AssayId,
    Guid ScreeningId,
    string Name,
    string AssayType,
    string? BiologicalTarget,
    string MeasurementUnit,
    decimal ActivityThreshold,
    string QualityControlStatus,
    decimal? ZPrimeFactor,
    IReadOnlyCollection<AssaySnapshotResult> Results)
    : AssayManagementIntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType =>
        nameof(AssaySnapshot);
}

public sealed record AssaySnapshotResult(
    Guid CompoundId,
    decimal MeanValue,
    decimal StandardDeviation,
    int ReplicateCount,
    decimal? Ic50,
    decimal? Ec50,
    decimal? Ki,
    decimal? Kd,
    decimal ConfidenceScore,
    string Outcome,
    bool SelectedForAi);
