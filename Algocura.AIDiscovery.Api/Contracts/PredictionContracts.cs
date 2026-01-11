using Algocura.AIDiscovery.Api.Domain;
using System.ComponentModel.DataAnnotations;

namespace Algocura.AIDiscovery.Api.Contracts;

public sealed record CreatePredictionRequest(
    [property: Required, MaxLength(200)] string Name,
    Guid ModelDefinitionId,
    Guid? SourceScreeningId,
    [property: MaxLength(1000)] string? Description,
    IReadOnlyCollection<Guid>? CompoundIds);

public sealed record FailPredictionRequest(
    [property: Required, MaxLength(2000)] string Reason);

public sealed record PredictionCandidateResponse(
    Guid CompoundId,
    string CompoundName,
    decimal ScreeningScore,
    int ScreeningRank,
    bool ScreeningSelected,
    decimal MolecularWeight);

public sealed record PredictionResultResponse(
    Guid CompoundId,
    decimal PredictionScore,
    int Rank,
    bool Prioritized,
    string? Explanation);

public sealed record PredictionResponse(
    Guid Id,
    string Name,
    Guid ModelDefinitionId,
    Guid? SourceScreeningId,
    string? Description,
    PredictionStatus Status,
    string? FailureReason,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyCollection<PredictionCandidateResponse> Candidates,
    IReadOnlyCollection<PredictionResultResponse> Results);

public sealed record ModelDefinitionResponse(
    Guid Id,
    string Name,
    string Version,
    string? Description,
    decimal ScreeningScoreWeight,
    decimal MolecularWeightWeight,
    decimal SelectionBonus,
    ModelStatus Status);
