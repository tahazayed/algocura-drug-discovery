using Algocura.AssayManagement.Api.Contracts;
using Algocura.AssayManagement.Api.Domain;

namespace Algocura.AssayManagement.Api.Mapping;

public static class AssayMappings
{
    public static CompletedScreeningResponse ToResponse(
        this CompletedScreeningProjection screening)
    {
        return new CompletedScreeningResponse(
            screening.ScreeningId,
            screening.Name,
            screening.ScreeningMethod,
            screening.CompletedAtUtc,
            screening.Status,
            screening.SourceUpdatedAtUtc,
            screening.SynchronizedAtUtc,
            screening.Results
                .OrderBy(x => x.Rank)
                .Select(x => new ScreeningResultResponse(
                    x.CompoundId,
                    x.Score,
                    x.Rank,
                    x.Selected))
                .ToArray());
    }

    public static AssayMeasurementResponse ToResponse(
        this AssayMeasurement measurement)
    {
        return new AssayMeasurementResponse(
            measurement.Id,
            measurement.CompoundId,
            measurement.MeasurementType,
            measurement.Concentration,
            measurement.ConcentrationUnit,
            measurement.ReplicateNumber,
            measurement.Value,
            measurement.IsControl,
            measurement.RecordedAtUtc);
    }

    public static AssayResultResponse ToResponse(
        this AssayResult result)
    {
        return new AssayResultResponse(
            result.Id,
            result.CompoundId,
            result.MeanValue,
            result.StandardDeviation,
            result.ReplicateCount,
            result.Ic50,
            result.Ec50,
            result.Ki,
            result.Kd,
            result.ConfidenceScore,
            result.Outcome,
            result.SelectedForAi,
            result.CreatedAtUtc);
    }

    public static AssayResponse ToResponse(
        this Assay assay)
    {
        return new AssayResponse(
            assay.Id,
            assay.ScreeningId,
            assay.Name,
            assay.AssayType,
            assay.BiologicalTarget,
            assay.Protocol,
            assay.MeasurementUnit,
            assay.ActivityThreshold,
            assay.Status,
            assay.QualityControlStatus,
            assay.ZPrimeFactor,
            assay.QualityControlNotes,
            assay.CreatedAtUtc,
            assay.UpdatedAtUtc,
            assay.CompletedAtUtc,
            assay.PublishedAtUtc,
            assay.Measurements
                .OrderBy(x => x.CompoundId)
                .ThenBy(x => x.ReplicateNumber)
                .Select(x => x.ToResponse())
                .ToArray(),
            assay.Results
                .OrderByDescending(x => x.MeanValue)
                .Select(x => x.ToResponse())
                .ToArray());
    }
}

