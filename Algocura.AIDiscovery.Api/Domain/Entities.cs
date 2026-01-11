using System.ComponentModel.DataAnnotations;

namespace Algocura.AIDiscovery.Api.Domain;

public enum PredictionStatus
{
    Draft = 1,
    Queued = 2,
    Running = 3,
    Completed = 4,
    Failed = 5,
    Cancelled = 6
}

public enum ModelStatus
{
    Draft = 1,
    Active = 2,
    Retired = 3
}

public sealed class CompoundProjection
{
    public Guid CompoundId { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(250)]
    public required string CanonicalSmiles { get; set; }

    [MaxLength(27)]
    public string? InchiKey { get; set; }

    public decimal MolecularWeight { get; set; }

    [MaxLength(100)]
    public string? MolecularFormula { get; set; }

    public int Status { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class ScreeningProjection
{
    public Guid ScreeningId { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(100)]
    public required string ScreeningMethod { get; set; }

    public DateTimeOffset CompletedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public List<ScreeningProjectionResult> Results { get; set; } = [];
}

public sealed class ScreeningProjectionResult
{
    public Guid Id { get; set; }

    public Guid ScreeningId { get; set; }

    public Guid CompoundId { get; set; }

    public decimal Score { get; set; }

    public int Rank { get; set; }

    public bool Selected { get; set; }

    public ScreeningProjection? Screening { get; set; }
}

public sealed class ModelDefinition
{
    public Guid Id { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(50)]
    public required string Version { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal ScreeningScoreWeight { get; set; }

    public decimal MolecularWeightWeight { get; set; }

    public decimal SelectionBonus { get; set; }

    public ModelStatus Status { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class PredictionJob
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    public Guid ModelDefinitionId { get; set; }

    public Guid? SourceScreeningId { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public PredictionStatus Status { get; set; }

    [MaxLength(2000)]
    public string? FailureReason { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public DateTimeOffset? StartedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }

    public ModelDefinition? ModelDefinition { get; set; }

    public List<PredictionCandidate> Candidates { get; set; } = [];

    public List<PredictionResult> Results { get; set; } = [];
}

public sealed class PredictionCandidate
{
    public Guid Id { get; set; }

    public Guid PredictionJobId { get; set; }

    public Guid CompoundId { get; set; }

    [MaxLength(100)]
    public required string CompoundName { get; set; }

    public decimal ScreeningScore { get; set; }

    public int ScreeningRank { get; set; }

    public bool ScreeningSelected { get; set; }

    public decimal MolecularWeight { get; set; }

    public PredictionJob? PredictionJob { get; set; }
}

public sealed class PredictionResult
{
    public Guid Id { get; set; }

    public Guid PredictionJobId { get; set; }

    public Guid CompoundId { get; set; }

    public decimal PredictionScore { get; set; }

    public int Rank { get; set; }

    public bool Prioritized { get; set; }

    [MaxLength(1000)]
    public string? Explanation { get; set; }

    public PredictionJob? PredictionJob { get; set; }
}
