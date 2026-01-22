using Algocura.Platform.Messaging.Events;

namespace Algocura.AIDiscovery.Api.Events;

public abstract record AIDiscoveryIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc)
    : IntegrationEvent(
        EventId,
        OccurredAtUtc)
{
    public override string Source =>
        "Algocura.AIDiscovery.Api";
}

public sealed record PredictionCreated(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid PredictionId,
    string Name,
    Guid ModelDefinitionId,
    int CandidateCount)
    : AIDiscoveryIntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType => nameof(PredictionCreated);
}

public sealed record PredictionStarted(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid PredictionId)
    : AIDiscoveryIntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType => nameof(PredictionStarted);
}

public sealed record PredictionCompleted(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid PredictionId,
    int ResultCount,
    int PrioritizedCount)
    : AIDiscoveryIntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType => nameof(PredictionCompleted);
}


public sealed record AdaptivePredictionCompleted(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid PredictionId,
    Guid FinalModelDefinitionId,
    decimal FinalConfidence,
    decimal ResourceConsumed,
    int ModelsExecuted)
    : AIDiscoveryIntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType => nameof(AdaptivePredictionCompleted);
}

public sealed record PredictionFailed(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid PredictionId,
    string Reason)
    : AIDiscoveryIntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType => nameof(PredictionFailed);
}

public sealed record CandidatePrioritized(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid PredictionId,
    Guid CompoundId,
    decimal PredictionScore,
    int Rank)
    : AIDiscoveryIntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType => nameof(CandidatePrioritized);
}

public sealed record CompoundRegistered
{
    public Guid EventId { get; init; }

    public DateTimeOffset OccurredAtUtc { get; init; }

    public Guid CompoundId { get; init; }

    public required string Name { get; init; }

    public required string CanonicalSmiles { get; init; }

    public string? InchiKey { get; init; }

    public decimal MolecularWeight { get; init; }

    public string? MolecularFormula { get; init; }

    public int Status { get; init; }

    public DateTimeOffset UpdatedAtUtc { get; init; }
}

public sealed record CompoundUpdated
{
    public Guid EventId { get; init; }

    public DateTimeOffset OccurredAtUtc { get; init; }

    public Guid CompoundId { get; init; }

    public required string Name { get; init; }

    public required string CanonicalSmiles { get; init; }

    public string? InchiKey { get; init; }

    public decimal MolecularWeight { get; init; }

    public string? MolecularFormula { get; init; }

    public int Status { get; init; }

    public DateTimeOffset UpdatedAtUtc { get; init; }
}

public sealed record CompoundDeleted
{
    public Guid EventId { get; init; }
    public DateTimeOffset OccurredAtUtc { get; init; }
    public Guid CompoundId { get; init; }
}

public sealed record ScreeningCompleted
{
    public Guid EventId { get; init; }
    public DateTimeOffset OccurredAtUtc { get; init; }
    public Guid ScreeningId { get; init; }
    public required string Name { get; init; }
    public required string ScreeningMethod { get; init; }
    public DateTimeOffset CompletedAtUtc { get; init; }
    public IReadOnlyCollection<ScreeningCompletedResult> Results { get; init; } =
        [];
}

public sealed record ScreeningCompletedResult
{
    public Guid CompoundId { get; init; }
    public decimal Score { get; init; }
    public int Rank { get; init; }
    public bool Selected { get; init; }
}
