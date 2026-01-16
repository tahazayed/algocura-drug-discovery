using Algocura.AIDiscovery.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.AIDiscovery.Api.Data;

public static class AIDiscoverySeeder
{
    private static readonly DateTimeOffset SeedTimestamp =
        new(
            2026,
            1,
            1,
            0,
            0,
            0,
            TimeSpan.Zero);

    public static readonly Guid PrototypeModelId =
        Guid.Parse("48d78c3a-0f59-4d43-baca-830c976988d6");

    // Matches the completed Screening API seed.
    private static readonly Guid ScreeningId =
        Guid.Parse("41000000-0000-0000-0000-000000000001");

    // Matches Compound Catalog and Screening API seed IDs.
    private static readonly Guid AspirinCompoundId =
        Guid.Parse("7f06dc5b-379f-4589-9384-4f09c55f97c3");

    private static readonly Guid CaffeineCompoundId =
        Guid.Parse("d2b92d72-5ce1-4659-aa35-9728dab134c6");

    private static readonly Guid AcetaminophenCompoundId =
        Guid.Parse("c0323692-4f01-4d97-87e9-a4720b3773bb");

    private static readonly Guid IbuprofenCompoundId =
        Guid.Parse("29dcde02-2703-42ee-ab50-33cf497058a5");

    private static readonly Guid NaproxenCompoundId =
        Guid.Parse("ad0df37e-aab8-4cd5-830b-bb5f03031143");

    private static readonly Guid AspirinScreeningResultId =
        Guid.Parse("51000000-0000-0000-0000-000000000001");

    private static readonly Guid NaproxenScreeningResultId =
        Guid.Parse("51000000-0000-0000-0000-000000000002");

    private static readonly Guid IbuprofenScreeningResultId =
        Guid.Parse("51000000-0000-0000-0000-000000000003");

    private static readonly Guid AcetaminophenScreeningResultId =
        Guid.Parse("51000000-0000-0000-0000-000000000004");

    private static readonly Guid CaffeineScreeningResultId =
        Guid.Parse("51000000-0000-0000-0000-000000000005");

    private static readonly Guid PredictionJobId =
        Guid.Parse("52000000-0000-0000-0000-000000000001");

    private static readonly Guid AspirinPredictionCandidateId =
        Guid.Parse("53000000-0000-0000-0000-000000000001");

    private static readonly Guid NaproxenPredictionCandidateId =
        Guid.Parse("53000000-0000-0000-0000-000000000002");

    private static readonly Guid IbuprofenPredictionCandidateId =
        Guid.Parse("53000000-0000-0000-0000-000000000003");

    private static readonly Guid AcetaminophenPredictionCandidateId =
        Guid.Parse("53000000-0000-0000-0000-000000000004");

    private static readonly Guid CaffeinePredictionCandidateId =
        Guid.Parse("53000000-0000-0000-0000-000000000005");

    private static readonly Guid AspirinPredictionResultId =
        Guid.Parse("54000000-0000-0000-0000-000000000001");

    private static readonly Guid NaproxenPredictionResultId =
        Guid.Parse("54000000-0000-0000-0000-000000000002");

    private static readonly Guid IbuprofenPredictionResultId =
        Guid.Parse("54000000-0000-0000-0000-000000000003");

    private static readonly Guid AcetaminophenPredictionResultId =
        Guid.Parse("54000000-0000-0000-0000-000000000004");

    private static readonly Guid CaffeinePredictionResultId =
        Guid.Parse("54000000-0000-0000-0000-000000000005");

    public static async Task SeedAsync(
        AIDiscoveryDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        await SeedCompoundsAsync(
            dbContext,
            cancellationToken);

        await SeedScreeningProjectionAsync(
            dbContext,
            cancellationToken);

        await SeedScreeningResultsAsync(
            dbContext,
            cancellationToken);

        await SeedModelDefinitionAsync(
            dbContext,
            cancellationToken);

        await SeedPredictionJobAsync(
            dbContext,
            cancellationToken);

        await SeedPredictionCandidatesAsync(
            dbContext,
            cancellationToken);

        await SeedPredictionResultsAsync(
            dbContext,
            cancellationToken);
    }

    private static async Task SeedCompoundsAsync(
        AIDiscoveryDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var compounds =
            new[]
            {
                CreateCompound(
                    AspirinCompoundId,
                    "Aspirin",
                    "CC(=O)OC1=CC=CC=C1C(=O)O",
                    "BSYNRYMUTXBXSQ-UHFFFAOYSA-N",
                    180.16m,
                    "C9H8O4",
                    2),

                CreateCompound(
                    CaffeineCompoundId,
                    "Caffeine",
                    "CN1C=NC2=C1C(=O)N(C(=O)N2C)C",
                    "RYYVLZVUVIJVGH-UHFFFAOYSA-N",
                    194.19m,
                    "C8H10N4O2",
                    3),

                CreateCompound(
                    AcetaminophenCompoundId,
                    "Acetaminophen",
                    "CC(=O)NC1=CC=C(C=C1)O",
                    "RZVAJINKPMORJF-UHFFFAOYSA-N",
                    151.16m,
                    "C8H9NO2",
                    2),

                CreateCompound(
                    IbuprofenCompoundId,
                    "Ibuprofen",
                    "CC(C)CC1=CC=C(C=C1)C(C)C(=O)O",
                    "HEFNNWSXXWATRW-UHFFFAOYSA-N",
                    206.28m,
                    "C13H18O2",
                    2),

                CreateCompound(
                    NaproxenCompoundId,
                    "Naproxen",
                    "COC1=CC2=C(C=C1)C=C(C=C2)[C@@H](C)C(=O)O",
                    "CMWTZPSULFXXJA-VIFPVBQESA-N",
                    230.26m,
                    "C14H14O3",
                    3)
            };

        var seedIds =
            compounds
                .Select(compound => compound.CompoundId)
                .ToArray();

        var existingIds =
            await dbContext.Compounds
                .AsNoTracking()
                .Where(compound =>
                    seedIds.Contains(compound.CompoundId))
                .Select(compound => compound.CompoundId)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var missingCompounds =
            compounds
                .Where(compound =>
                    !existingIdSet.Contains(compound.CompoundId))
                .ToArray();

        if (missingCompounds.Length == 0)
        {
            return;
        }

        await dbContext.Compounds.AddRangeAsync(
            missingCompounds,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedScreeningProjectionAsync(
        AIDiscoveryDbContext dbContext,
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
            new ScreeningProjection
            {
                ScreeningId =
                    ScreeningId,

                Name =
                    "Prototype TREM2 compound screening",

                ScreeningMethod =
                    "Deterministic prototype scoring",

                CompletedAtUtc =
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

    private static async Task SeedScreeningResultsAsync(
        AIDiscoveryDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var results =
            new[]
            {
                CreateScreeningResult(
                    AspirinScreeningResultId,
                    AspirinCompoundId,
                    0.93m,
                    1,
                    true),

                CreateScreeningResult(
                    NaproxenScreeningResultId,
                    NaproxenCompoundId,
                    0.87m,
                    2,
                    true),

                CreateScreeningResult(
                    IbuprofenScreeningResultId,
                    IbuprofenCompoundId,
                    0.79m,
                    3,
                    false),

                CreateScreeningResult(
                    AcetaminophenScreeningResultId,
                    AcetaminophenCompoundId,
                    0.68m,
                    4,
                    false),

                CreateScreeningResult(
                    CaffeineScreeningResultId,
                    CaffeineCompoundId,
                    0.51m,
                    5,
                    false)
            };

        var seedIds =
            results
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

        var missingResults =
            results
                .Where(result =>
                    !existingIdSet.Contains(result.Id))
                .ToArray();

        if (missingResults.Length == 0)
        {
            return;
        }

        await dbContext.ScreeningResults.AddRangeAsync(
            missingResults,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedModelDefinitionAsync(
        AIDiscoveryDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists =
            await dbContext.ModelDefinitions
                .AsNoTracking()
                .AnyAsync(
                    model =>
                        model.Id ==
                        PrototypeModelId,
                    cancellationToken);

        if (exists)
        {
            return;
        }

        var model =
            new ModelDefinition
            {
                Id =
                    PrototypeModelId,

                Name =
                    "Algocura Deterministic Prioritizer",

                Version =
                    "1.0",

                Description =
                    "Prototype deterministic scoring model used to rank " +
                    "screened compounds for downstream discovery review.",

                ScreeningScoreWeight =
                    0.75m,

                MolecularWeightWeight =
                    0.15m,

                SelectionBonus =
                    10m,

                Status =
                    ModelStatus.Active,

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
            };

        await dbContext.ModelDefinitions.AddAsync(
            model,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedPredictionJobAsync(
        AIDiscoveryDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists =
            await dbContext.PredictionJobs
                .AsNoTracking()
                .AnyAsync(
                    job =>
                        job.Id ==
                        PredictionJobId,
                    cancellationToken);

        if (exists)
        {
            return;
        }

        var job =
            new PredictionJob
            {
                Id =
                    PredictionJobId,

                Name =
                    "Prototype TREM2 candidate prioritization",

                ModelDefinitionId =
                    PrototypeModelId,

                SourceScreeningId =
                    ScreeningId,

                Description =
                    "Prototype AI Discovery job that prioritizes compounds " +
                    "using screening scores, molecular-weight contribution, " +
                    "and a screening-selection bonus.",

                Status =
                    PredictionStatus.Completed,

                FailureReason =
                    null,

                StartedAtUtc =
                    SeedTimestamp,

                StartedByUserId =
                    SystemUsers.SystemUserId,

                CompletedByUserId =
                    SystemUsers.SystemUserId,

                CompletedAtUtc =
                    SeedTimestamp,

                OwnerUserId =
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

        await dbContext.PredictionJobs.AddAsync(
            job,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedPredictionCandidatesAsync(
        AIDiscoveryDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var candidates =
            new[]
            {
                CreatePredictionCandidate(
                    AspirinPredictionCandidateId,
                    AspirinCompoundId,
                    "Aspirin",
                    0.93m,
                    1,
                    true,
                    180.16m),

                CreatePredictionCandidate(
                    NaproxenPredictionCandidateId,
                    NaproxenCompoundId,
                    "Naproxen",
                    0.87m,
                    2,
                    true,
                    230.26m),

                CreatePredictionCandidate(
                    IbuprofenPredictionCandidateId,
                    IbuprofenCompoundId,
                    "Ibuprofen",
                    0.79m,
                    3,
                    false,
                    206.28m),

                CreatePredictionCandidate(
                    AcetaminophenPredictionCandidateId,
                    AcetaminophenCompoundId,
                    "Acetaminophen",
                    0.68m,
                    4,
                    false,
                    151.16m),

                CreatePredictionCandidate(
                    CaffeinePredictionCandidateId,
                    CaffeineCompoundId,
                    "Caffeine",
                    0.51m,
                    5,
                    false,
                    194.19m)
            };

        var seedIds =
            candidates
                .Select(candidate => candidate.Id)
                .ToArray();

        var existingIds =
            await dbContext.PredictionCandidates
                .AsNoTracking()
                .Where(candidate =>
                    seedIds.Contains(candidate.Id))
                .Select(candidate => candidate.Id)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var missingCandidates =
            candidates
                .Where(candidate =>
                    !existingIdSet.Contains(candidate.Id))
                .ToArray();

        if (missingCandidates.Length == 0)
        {
            return;
        }

        await dbContext.PredictionCandidates.AddRangeAsync(
            missingCandidates,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedPredictionResultsAsync(
        AIDiscoveryDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var results =
            new[]
            {
                CreatePredictionResult(
                    AspirinPredictionResultId,
                    AspirinCompoundId,
                    0.96m,
                    1,
                    true,
                    "Highest prototype prediction score based on strong " +
                    "screening performance and selection bonus."),

                CreatePredictionResult(
                    NaproxenPredictionResultId,
                    NaproxenCompoundId,
                    0.91m,
                    2,
                    true,
                    "Second-ranked prototype candidate with strong screening " +
                    "performance and selection bonus."),

                CreatePredictionResult(
                    IbuprofenPredictionResultId,
                    IbuprofenCompoundId,
                    0.76m,
                    3,
                    false,
                    "Moderate prediction score without a screening-selection bonus."),

                CreatePredictionResult(
                    AcetaminophenPredictionResultId,
                    AcetaminophenCompoundId,
                    0.66m,
                    4,
                    false,
                    "Lower prediction score retained for comparison."),

                CreatePredictionResult(
                    CaffeinePredictionResultId,
                    CaffeineCompoundId,
                    0.52m,
                    5,
                    false,
                    "Lowest prototype prediction score in this seeded job.")
            };

        var seedIds =
            results
                .Select(result => result.Id)
                .ToArray();

        var existingIds =
            await dbContext.PredictionResults
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

        await dbContext.PredictionResults.AddRangeAsync(
            missingResults,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static CompoundProjection CreateCompound(
        Guid compoundId,
        string name,
        string canonicalSmiles,
        string inchiKey,
        decimal molecularWeight,
        string molecularFormula,
        int status)
    {
        return new CompoundProjection
        {
            CompoundId =
                compoundId,

            Name =
                name,

            CanonicalSmiles =
                canonicalSmiles,

            InchiKey =
                inchiKey,

            MolecularWeight =
                molecularWeight,

            MolecularFormula =
                molecularFormula,

            Status =
                status,

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

    private static ScreeningProjectionResult CreateScreeningResult(
        Guid id,
        Guid compoundId,
        decimal score,
        int rank,
        bool selected)
    {
        return new ScreeningProjectionResult
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

    private static PredictionCandidate CreatePredictionCandidate(
        Guid id,
        Guid compoundId,
        string compoundName,
        decimal screeningScore,
        int screeningRank,
        bool screeningSelected,
        decimal molecularWeight)
    {
        return new PredictionCandidate
        {
            Id =
                id,

            PredictionJobId =
                PredictionJobId,

            CompoundId =
                compoundId,

            CompoundName =
                compoundName,

            ScreeningScore =
                screeningScore,

            ScreeningRank =
                screeningRank,

            ScreeningSelected =
                screeningSelected,

            MolecularWeight =
                molecularWeight,

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

    private static PredictionResult CreatePredictionResult(
        Guid id,
        Guid compoundId,
        decimal predictionScore,
        int rank,
        bool prioritized,
        string explanation)
    {
        return new PredictionResult
        {
            Id =
                id,

            PredictionJobId =
                PredictionJobId,

            CompoundId =
                compoundId,

            PredictionScore =
                predictionScore,

            Rank =
                rank,

            Prioritized =
                prioritized,

            PrioritizedByUserId =
                prioritized
                    ? SystemUsers.SystemUserId
                    : null,

            PrioritizedAtUtc =
                prioritized
                    ? SeedTimestamp
                    : null,

            ApprovedByUserId =
                prioritized
                    ? SystemUsers.SystemUserId
                    : null,

            ApprovedAtUtc =
                prioritized
                    ? SeedTimestamp
                    : null,

            Explanation =
                explanation,

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
