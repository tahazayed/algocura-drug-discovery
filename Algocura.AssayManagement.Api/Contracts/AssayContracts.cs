using Algocura.AssayManagement.Api.Domain;
using System.ComponentModel.DataAnnotations;

namespace Algocura.AssayManagement.Api.Contracts;

public sealed record CreateAssayRequest(
    Guid ScreeningId,
    [property: Required, MaxLength(200)] string Name,
    AssayType AssayType,
    [property: MaxLength(200)] string? BiologicalTarget,
    [property: MaxLength(2000)] string? Protocol,
    [property: Required, MaxLength(100)] string MeasurementUnit,
    decimal ActivityThreshold);

public sealed record AddAssayMeasurementRequest(
    Guid CompoundId,
    MeasurementType MeasurementType,
    decimal Concentration,
    [property: Required, MaxLength(50)] string ConcentrationUnit,
    int ReplicateNumber,
    decimal Value,
    bool IsControl);

public sealed record CompleteAssayRequest(
    decimal? ZPrimeFactor,
    QualityControlStatus QualityControlStatus,
    [property: MaxLength(1000)] string? QualityControlNotes);

public sealed record ScreeningResultResponse(
    Guid CompoundId,
    decimal Score,
    int Rank,
    bool Selected);

public sealed record CompletedScreeningResponse(
    Guid ScreeningId,
    string Name,
    string ScreeningMethod,
    DateTimeOffset CompletedAtUtc,
    ScreeningProjectionStatus Status,
    DateTimeOffset SourceUpdatedAtUtc,
    DateTimeOffset SynchronizedAtUtc,
    IReadOnlyCollection<ScreeningResultResponse> Results);

public sealed record AssayMeasurementResponse(
    Guid Id,
    Guid CompoundId,
    MeasurementType MeasurementType,
    decimal Concentration,
    string ConcentrationUnit,
    int ReplicateNumber,
    decimal Value,
    bool IsControl,
    DateTimeOffset RecordedAtUtc);

public sealed record AssayResultResponse(
    Guid Id,
    Guid CompoundId,
    decimal MeanValue,
    decimal StandardDeviation,
    int ReplicateCount,
    decimal? Ic50,
    decimal? Ec50,
    decimal? Ki,
    decimal? Kd,
    decimal ConfidenceScore,
    AssayOutcome Outcome,
    bool SelectedForAi,
    DateTimeOffset CreatedAtUtc);

public sealed record AssayResponse(
    Guid Id,
    Guid ScreeningId,
    string Name,
    AssayType AssayType,
    string? BiologicalTarget,
    string? Protocol,
    string MeasurementUnit,
    decimal ActivityThreshold,
    AssayStatus Status,
    QualityControlStatus QualityControlStatus,
    decimal? ZPrimeFactor,
    string? QualityControlNotes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? PublishedAtUtc,
    IReadOnlyCollection<AssayMeasurementResponse> Measurements,
    IReadOnlyCollection<AssayResultResponse> Results);

