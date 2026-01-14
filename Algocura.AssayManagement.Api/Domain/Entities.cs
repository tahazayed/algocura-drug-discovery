using System.ComponentModel.DataAnnotations;

namespace Algocura.AssayManagement.Api.Domain;

public enum ScreeningProjectionStatus
{
    Completed = 1,
    Archived = 2
}

public enum AssayType
{
    Biochemical = 1,
    CellBased = 2,
    Binding = 3,
    Functional = 4,
    Toxicity = 5,
    Selectivity = 6,
    Admet = 7
}

public enum AssayStatus
{
    Draft = 1,
    InProgress = 2,
    Completed = 3,
    Rejected = 4,
    Published = 5
}

public enum MeasurementType
{
    PercentInhibition = 1,
    PercentActivation = 2,
    Viability = 3,
    BindingResponse = 4,
    SignalIntensity = 5,
    ConcentrationResponse = 6
}

public enum QualityControlStatus
{
    Pending = 1,
    Passed = 2,
    Failed = 3
}

public enum AssayOutcome
{
    Inconclusive = 1,
    Inactive = 2,
    Active = 3,
    Potent = 4,
    Toxic = 5
}

public sealed class CompletedScreeningProjection
{
    public Guid ScreeningId { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(100)]
    public required string ScreeningMethod { get; set; }

    public DateTimeOffset CompletedAtUtc { get; set; }

    public ScreeningProjectionStatus Status { get; set; }

    public DateTimeOffset SourceUpdatedAtUtc { get; set; }

    public DateTimeOffset SynchronizedAtUtc { get; set; }

    public List<CompletedScreeningResultProjection> Results { get; set; } = [];

    public List<Assay> Assays { get; set; } = [];
}

public sealed class CompletedScreeningResultProjection
{
    public Guid Id { get; set; }

    public Guid ScreeningId { get; set; }

    public Guid CompoundId { get; set; }

    public decimal Score { get; set; }

    public int Rank { get; set; }

    public bool Selected { get; set; }

    public CompletedScreeningProjection? Screening { get; set; }
}

public sealed class Assay
{
    public Guid Id { get; set; }

    public Guid ScreeningId { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    public AssayType AssayType { get; set; }

    [MaxLength(200)]
    public string? BiologicalTarget { get; set; }

    [MaxLength(2000)]
    public string? Protocol { get; set; }

    [MaxLength(100)]
    public required string MeasurementUnit { get; set; }

    public decimal ActivityThreshold { get; set; }

    public AssayStatus Status { get; set; }

    public QualityControlStatus QualityControlStatus { get; set; }

    public decimal? ZPrimeFactor { get; set; }

    [MaxLength(1000)]
    public string? QualityControlNotes { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }

    public DateTimeOffset? PublishedAtUtc { get; set; }

    public CompletedScreeningProjection? Screening { get; set; }

    public List<AssayMeasurement> Measurements { get; set; } = [];

    public List<AssayResult> Results { get; set; } = [];
}

public sealed class AssayMeasurement
{
    public Guid Id { get; set; }

    public Guid AssayId { get; set; }

    public Guid CompoundId { get; set; }

    public MeasurementType MeasurementType { get; set; }

    public decimal Concentration { get; set; }

    [MaxLength(50)]
    public required string ConcentrationUnit { get; set; }

    public int ReplicateNumber { get; set; }

    public decimal Value { get; set; }

    public bool IsControl { get; set; }

    public DateTimeOffset RecordedAtUtc { get; set; }

    public Assay? Assay { get; set; }
}

public sealed class AssayResult
{
    public Guid Id { get; set; }

    public Guid AssayId { get; set; }

    public Guid CompoundId { get; set; }

    public decimal MeanValue { get; set; }

    public decimal StandardDeviation { get; set; }

    public int ReplicateCount { get; set; }

    public decimal? Ic50 { get; set; }

    public decimal? Ec50 { get; set; }

    public decimal? Ki { get; set; }

    public decimal? Kd { get; set; }

    public decimal ConfidenceScore { get; set; }

    public AssayOutcome Outcome { get; set; }

    public bool SelectedForAi { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public Assay? Assay { get; set; }
}

