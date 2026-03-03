using Algocura.AssayManagement.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.AssayManagement.Api.Data;

public static class AssayManagementSeeder
{
    private static readonly DateTimeOffset SeedTimestamp =
        new(
            2026,
            1,
            12,
            0,
            0,
            0,
            TimeSpan.Zero);

    // Matches the completed screening seeded by Algocura.Screening.Api.
    private static readonly Guid ScreeningId =
        Guid.Parse("41000000-0000-0000-0000-000000000001");

    // Matches Compound Catalog and Screening seed IDs.
    private static readonly Guid AspirinCompoundId =
        Guid.Parse("7f06dc5b-379f-4589-9384-4f09c55f97c3");

    private static readonly Guid NaproxenCompoundId =
        Guid.Parse("ad0df37e-aab8-4cd5-830b-bb5f03031143");

    private static readonly Guid AspirinScreeningResultProjectionId =
        Guid.Parse("61000000-0000-0000-0000-000000000001");

    private static readonly Guid NaproxenScreeningResultProjectionId =
        Guid.Parse("61000000-0000-0000-0000-000000000002");

    private static readonly Guid AssayId =
        Guid.Parse("62000000-0000-0000-0000-000000000001");

    private static readonly Guid AspirinMeasurementOneId =
        Guid.Parse("63000000-0000-0000-0000-000000000001");

    private static readonly Guid AspirinMeasurementTwoId =
        Guid.Parse("63000000-0000-0000-0000-000000000002");

    private static readonly Guid AspirinMeasurementThreeId =
        Guid.Parse("63000000-0000-0000-0000-000000000003");

    private static readonly Guid NaproxenMeasurementOneId =
        Guid.Parse("63000000-0000-0000-0000-000000000004");

    private static readonly Guid NaproxenMeasurementTwoId =
        Guid.Parse("63000000-0000-0000-0000-000000000005");

    private static readonly Guid NaproxenMeasurementThreeId =
        Guid.Parse("63000000-0000-0000-0000-000000000006");

    private static readonly Guid AspirinAssayResultId =
        Guid.Parse("64000000-0000-0000-0000-000000000001");

    private static readonly Guid NaproxenAssayResultId =
        Guid.Parse("64000000-0000-0000-0000-000000000002");

    public static async Task SeedAsync(
        AssayManagementDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        await SeedCompletedScreeningAsync(
            dbContext,
            cancellationToken);

        await SeedCompletedScreeningResultsAsync(
            dbContext,
            cancellationToken);

        await SeedAssayAsync(
            dbContext,
            cancellationToken);

        await SeedMeasurementsAsync(
            dbContext,
            cancellationToken);

        await SeedAssayResultsAsync(
            dbContext,
            cancellationToken);
    }

    private static async Task SeedCompletedScreeningAsync(
        AssayManagementDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists =
            await dbContext.Screenings
                .AsNoTracking()
                .AnyAsync(
                    screening =>
                        screening.ScreeningId ==
                        ScreeningId,
                    cancellationToken);

        if (exists)
        {
            return;
        }

        var screening =
            new CompletedScreeningProjection
            {
                ScreeningId =
                    ScreeningId,

                Name =
                    "Prototype TREM2 compound screening",

                ScreeningMethod =
                    "Deterministic prototype scoring",

                CompletedAtUtc =
                    SeedTimestamp,

                Status =
                    ScreeningProjectionStatus.Completed,

                SourceUpdatedAtUtc =
                    SeedTimestamp,

                SynchronizedAtUtc =
                    SeedTimestamp,

                SourceCreatedByUserId =
                    SystemUsers.SystemUserId,

                SourceUpdatedByUserId =
                    SystemUsers.SystemUserId,

                CreatedByUserId =
                    SystemUsers.SystemUserId,

                UpdatedByUserId =
                    SystemUsers.SystemUserId,

                CreatedAtUtc =
                    SeedTimestamp,

                UpdatedAtUtc =
                    SeedTimestamp
            };

        await dbContext.Screenings.AddAsync(
            screening,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedCompletedScreeningResultsAsync(
        AssayManagementDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var projections =
            new[]
            {
                CreateCompletedScreeningResult(
                    AspirinScreeningResultProjectionId,
                    AspirinCompoundId,
                    0.93m,
                    1,
                    true),

                CreateCompletedScreeningResult(
                    NaproxenScreeningResultProjectionId,
                    NaproxenCompoundId,
                    0.87m,
                    2,
                    true)
            };

        var seedIds =
            projections
                .Select(result => result.Id)
                .ToArray();

        var existingIds =
            await dbContext.ScreeningResults
                .AsNoTracking()
                .Where(result =>
                    seedIds.Contains(result.Id))
                .Select(result => result.Id)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var missingProjections =
            projections
                .Where(result =>
                    !existingIdSet.Contains(result.Id))
                .ToArray();

        if (missingProjections.Length == 0)
        {
            return;
        }

        await dbContext.ScreeningResults.AddRangeAsync(
            missingProjections,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedAssayAsync(
        AssayManagementDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists =
            await dbContext.Assays
                .AsNoTracking()
                .AnyAsync(
                    assay =>
                        assay.Id ==
                        AssayId,
                    cancellationToken);

        if (exists)
        {
            return;
        }

        var assay =
            new Assay
            {
                Id =
                    AssayId,

                ScreeningId =
                    ScreeningId,

                Name =
                    "Prototype TREM2 functional activity assay",

                AssayType =
                    AssayType.Functional,

                BiologicalTarget =
                    "Triggering receptor expressed on myeloid cells 2",

                Protocol =
                    "Prototype cell-based functional assay used to compare " +
                    "the activity of selected screening compounds. " +
                    "This seed record is for platform demonstration only.",

                MeasurementUnit =
                    "Percent activation",

                ActivityThreshold =
                    60.00m,

                Status =
                    AssayStatus.Published,

                QualityControlStatus =
                    QualityControlStatus.Passed,

                ZPrimeFactor =
                    0.71m,

                QualityControlNotes =
                    "Prototype quality-control checks passed.",

                OwnerUserId =
                    SystemUsers.SystemUserId,

                CompletedByUserId =
                    SystemUsers.SystemUserId,

                CompletedAtUtc =
                    SeedTimestamp,

                ApprovedByUserId =
                    SystemUsers.SystemUserId,

                ApprovedAtUtc =
                    SeedTimestamp,

                PublishedByUserId =
                    SystemUsers.SystemUserId,

                PublishedAtUtc =
                    SeedTimestamp,

                CreatedByUserId =
                    SystemUsers.SystemUserId,

                UpdatedByUserId =
                    SystemUsers.SystemUserId,

                CreatedAtUtc =
                    SeedTimestamp,

                UpdatedAtUtc =
                    SeedTimestamp
            };

        await dbContext.Assays.AddAsync(
            assay,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedMeasurementsAsync(
        AssayManagementDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var measurements =
            new[]
            {
                CreateMeasurement(
                    AspirinMeasurementOneId,
                    AspirinCompoundId,
                    1,
                    72.10m),

                CreateMeasurement(
                    AspirinMeasurementTwoId,
                    AspirinCompoundId,
                    2,
                    74.30m),

                CreateMeasurement(
                    AspirinMeasurementThreeId,
                    AspirinCompoundId,
                    3,
                    73.20m),

                CreateMeasurement(
                    NaproxenMeasurementOneId,
                    NaproxenCompoundId,
                    1,
                    65.40m),

                CreateMeasurement(
                    NaproxenMeasurementTwoId,
                    NaproxenCompoundId,
                    2,
                    67.10m),

                CreateMeasurement(
                    NaproxenMeasurementThreeId,
                    NaproxenCompoundId,
                    3,
                    66.20m)
            };

        var seedIds =
            measurements
                .Select(measurement => measurement.Id)
                .ToArray();

        var existingIds =
            await dbContext.AssayMeasurements
                .AsNoTracking()
                .Where(measurement =>
                    seedIds.Contains(measurement.Id))
                .Select(measurement => measurement.Id)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var missingMeasurements =
            measurements
                .Where(measurement =>
                    !existingIdSet.Contains(measurement.Id))
                .ToArray();

        if (missingMeasurements.Length == 0)
        {
            return;
        }

        await dbContext.AssayMeasurements.AddRangeAsync(
            missingMeasurements,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedAssayResultsAsync(
        AssayManagementDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var results =
            new[]
            {
                new AssayResult
                {
                    Id =
                        AspirinAssayResultId,

                    AssayId =
                        AssayId,

                    CompoundId =
                        AspirinCompoundId,

                    MeanValue =
                        73.20m,

                    StandardDeviation =
                        1.10m,

                    ReplicateCount =
                        3,

                    Ic50 =
                        null,

                    Ec50 =
                        2.45m,

                    Ki =
                        null,

                    Kd =
                        null,

                    ConfidenceScore =
                        0.94m,

                    Outcome =
                        AssayOutcome.Potent,

                    SelectedForAi =
                        true,

                    SelectedForAiByUserId =
                        SystemUsers.SystemUserId,

                    SelectedForAiAtUtc =
                        SeedTimestamp,

                    ApprovedByUserId =
                        SystemUsers.SystemUserId,

                    ApprovedAtUtc =
                        SeedTimestamp,

                    CreatedByUserId =
                        SystemUsers.SystemUserId,

                    UpdatedByUserId =
                        SystemUsers.SystemUserId,

                    CreatedAtUtc =
                        SeedTimestamp,

                    UpdatedAtUtc =
                        SeedTimestamp
                },

                new AssayResult
                {
                    Id =
                        NaproxenAssayResultId,

                    AssayId =
                        AssayId,

                    CompoundId =
                        NaproxenCompoundId,

                    MeanValue =
                        66.23m,

                    StandardDeviation =
                        0.85m,

                    ReplicateCount =
                        3,

                    Ic50 =
                        null,

                    Ec50 =
                        4.10m,

                    Ki =
                        null,

                    Kd =
                        null,

                    ConfidenceScore =
                        0.89m,

                    Outcome =
                        AssayOutcome.Active,

                    SelectedForAi =
                        true,

                    SelectedForAiByUserId =
                        SystemUsers.SystemUserId,

                    SelectedForAiAtUtc =
                        SeedTimestamp,

                    ApprovedByUserId =
                        SystemUsers.SystemUserId,

                    ApprovedAtUtc =
                        SeedTimestamp,

                    CreatedByUserId =
                        SystemUsers.SystemUserId,

                    UpdatedByUserId =
                        SystemUsers.SystemUserId,

                    CreatedAtUtc =
                        SeedTimestamp,

                    UpdatedAtUtc =
                        SeedTimestamp
                }
            };

        var seedIds =
            results
                .Select(result => result.Id)
                .ToArray();

        var existingIds =
            await dbContext.AssayResults
                .AsNoTracking()
                .Where(result =>
                    seedIds.Contains(result.Id))
                .Select(result => result.Id)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var missingResults =
            results
                .Where(result =>
                    !existingIdSet.Contains(result.Id))
                .ToArray();

        if (missingResults.Length == 0)
        {
            return;
        }

        await dbContext.AssayResults.AddRangeAsync(
            missingResults,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static CompletedScreeningResultProjection
        CreateCompletedScreeningResult(
            Guid id,
            Guid compoundId,
            decimal score,
            int rank,
            bool selected)
    {
        return new CompletedScreeningResultProjection
        {
            Id =
                id,

            ScreeningId =
                ScreeningId,

            CompoundId =
                compoundId,

            Score =
                score,

            Rank =
                rank,

            Selected =
                selected,

            SourceCreatedByUserId =
                SystemUsers.SystemUserId,

            SourceUpdatedByUserId =
                SystemUsers.SystemUserId,

            CreatedByUserId =
                SystemUsers.SystemUserId,

            UpdatedByUserId =
                SystemUsers.SystemUserId,

            CreatedAtUtc =
                SeedTimestamp,

            UpdatedAtUtc =
                SeedTimestamp
        };
    }

    private static AssayMeasurement CreateMeasurement(
        Guid id,
        Guid compoundId,
        int replicateNumber,
        decimal value)
    {
        return new AssayMeasurement
        {
            Id =
                id,

            AssayId =
                AssayId,

            CompoundId =
                compoundId,

            MeasurementType =
                MeasurementType.PercentActivation,

            Concentration =
                10.00m,

            ConcentrationUnit =
                "uM",

            ReplicateNumber =
                replicateNumber,

            Value =
                value,

            IsControl =
                false,

            RecordedAtUtc =
                SeedTimestamp,

            RecordedByUserId =
                SystemUsers.SystemUserId,

            CorrectedByUserId =
                null,

            CorrectedAtUtc =
                null,

            CreatedByUserId =
                SystemUsers.SystemUserId,

            UpdatedByUserId =
                SystemUsers.SystemUserId,

            CreatedAtUtc =
                SeedTimestamp,

            UpdatedAtUtc =
                SeedTimestamp
        };
    }
}
