using Algocura.Screening.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Screening.Api.Data;

public static class ScreeningSeeder
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

    // Matches the Compound Catalog seed IDs.
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

    private static readonly Guid ScreeningJobId =
        Guid.Parse("41000000-0000-0000-0000-000000000001");

    private static readonly Guid AspirinCandidateId =
        Guid.Parse("42000000-0000-0000-0000-000000000001");

    private static readonly Guid CaffeineCandidateId =
        Guid.Parse("42000000-0000-0000-0000-000000000002");

    private static readonly Guid AcetaminophenCandidateId =
        Guid.Parse("42000000-0000-0000-0000-000000000003");

    private static readonly Guid IbuprofenCandidateId =
        Guid.Parse("42000000-0000-0000-0000-000000000004");

    private static readonly Guid NaproxenCandidateId =
        Guid.Parse("42000000-0000-0000-0000-000000000005");

    private static readonly Guid AspirinResultId =
        Guid.Parse("43000000-0000-0000-0000-000000000001");

    private static readonly Guid NaproxenResultId =
        Guid.Parse("43000000-0000-0000-0000-000000000002");

    private static readonly Guid IbuprofenResultId =
        Guid.Parse("43000000-0000-0000-0000-000000000003");

    private static readonly Guid AcetaminophenResultId =
        Guid.Parse("43000000-0000-0000-0000-000000000004");

    private static readonly Guid CaffeineResultId =
        Guid.Parse("43000000-0000-0000-0000-000000000005");

    public static async Task SeedAsync(
        ScreeningDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        await SeedCompoundsAsync(
            dbContext,
            cancellationToken);

        await SeedScreeningJobAsync(
            dbContext,
            cancellationToken);

        await SeedCandidatesAsync(
            dbContext,
            cancellationToken);

        await SeedResultsAsync(
            dbContext,
            cancellationToken);
    }

    private static async Task SeedCompoundsAsync(
        ScreeningDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var compounds =
            new[]
            {
                CreateCompound(
                    AspirinCompoundId,
                    "Aspirin",
                    "CC(=O)OC1=CC=CC=C1C(=O)O",
                    "BSYNRYMUTXBXSQ-UHFFFAOYSA-N"),

                CreateCompound(
                    CaffeineCompoundId,
                    "Caffeine",
                    "CN1C=NC2=C1C(=O)N(C(=O)N2C)C",
                    "RYYVLZVUVIJVGH-UHFFFAOYSA-N"),

                CreateCompound(
                    AcetaminophenCompoundId,
                    "Acetaminophen",
                    "CC(=O)NC1=CC=C(C=C1)O",
                    "RZVAJINKPMORJF-UHFFFAOYSA-N"),

                CreateCompound(
                    IbuprofenCompoundId,
                    "Ibuprofen",
                    "CC(C)CC1=CC=C(C=C1)C(C)C(=O)O",
                    "HEFNNWSXXWATRW-UHFFFAOYSA-N"),

                CreateCompound(
                    NaproxenCompoundId,
                    "Naproxen",
                    "COC1=CC2=C(C=C1)C=C(C=C2)[C@@H](C)C(=O)O",
                    "CMWTZPSULFXXJA-VIFPVBQESA-N")
            };

        var seedIds =
            compounds
                .Select(compound => compound.CompoundId)
                .ToArray();

        var existingIds =
            await dbContext.ScreeningCompounds
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

        await dbContext.ScreeningCompounds.AddRangeAsync(
            missingCompounds,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedScreeningJobAsync(
        ScreeningDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists =
            await dbContext.ScreeningJobs
                .AsNoTracking()
                .AnyAsync(
                    job =>
                        job.Id ==
                        ScreeningJobId,
                    cancellationToken);

        if (exists)
        {
            return;
        }

        var job =
            new ScreeningJob
            {
                Id =
                    ScreeningJobId,

                Name =
                    "Prototype TREM2 compound screening",

                Description =
                    "Prototype screening job that evaluates selected " +
                    "catalog compounds for downstream assay processing.",

                ScreeningMethod =
                    "Deterministic prototype scoring",

                Status =
                    ScreeningStatus.Completed,

                StartedAtUtc =
                    SeedTimestamp,

                StartedByUserId =
                    SystemUsers.SystemUserId,

                CompletedByUserId =
                    SystemUsers.SystemUserId,

                CompletedAtUtc =
                    SeedTimestamp,

                FailureReason =
                    null,

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

        await dbContext.ScreeningJobs.AddAsync(
            job,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedCandidatesAsync(
        ScreeningDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var candidates =
            new[]
            {
                CreateCandidate(
                    AspirinCandidateId,
                    AspirinCompoundId,
                    "Aspirin",
                    "CC(=O)OC1=CC=CC=C1C(=O)O"),

                CreateCandidate(
                    CaffeineCandidateId,
                    CaffeineCompoundId,
                    "Caffeine",
                    "CN1C=NC2=C1C(=O)N(C(=O)N2C)C"),

                CreateCandidate(
                    AcetaminophenCandidateId,
                    AcetaminophenCompoundId,
                    "Acetaminophen",
                    "CC(=O)NC1=CC=C(C=C1)O"),

                CreateCandidate(
                    IbuprofenCandidateId,
                    IbuprofenCompoundId,
                    "Ibuprofen",
                    "CC(C)CC1=CC=C(C=C1)C(C)C(=O)O"),

                CreateCandidate(
                    NaproxenCandidateId,
                    NaproxenCompoundId,
                    "Naproxen",
                    "COC1=CC2=C(C=C1)C=C(C=C2)[C@@H](C)C(=O)O")
            };

        var seedIds =
            candidates
                .Select(candidate => candidate.Id)
                .ToArray();

        var existingIds =
            await dbContext.ScreeningCandidates
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

        await dbContext.ScreeningCandidates.AddRangeAsync(
            missingCandidates,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedResultsAsync(
        ScreeningDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var results =
            new[]
            {
                CreateResult(
                    AspirinResultId,
                    AspirinCompoundId,
                    0.93m,
                    1,
                    true,
                    "Highest prototype score; selected for assay processing."),

                CreateResult(
                    NaproxenResultId,
                    NaproxenCompoundId,
                    0.87m,
                    2,
                    true,
                    "Secondary prototype hit selected for assay processing."),

                CreateResult(
                    IbuprofenResultId,
                    IbuprofenCompoundId,
                    0.79m,
                    3,
                    false,
                    "Retained as a non-selected comparison compound."),

                CreateResult(
                    AcetaminophenResultId,
                    AcetaminophenCompoundId,
                    0.68m,
                    4,
                    false,
                    "Moderate prototype score."),

                CreateResult(
                    CaffeineResultId,
                    CaffeineCompoundId,
                    0.51m,
                    5,
                    false,
                    "Lower prototype score.")
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

    private static ScreeningCompound CreateCompound(
        Guid compoundId,
        string name,
        string canonicalSmiles,
        string inchiKey)
    {
        return new ScreeningCompound
        {
            CompoundId =
                compoundId,

            Name =
                name,

            CanonicalSmiles =
                canonicalSmiles,

            InchiKey =
                inchiKey,

            RegisteredAtUtc =
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
    }

    private static ScreeningCandidate CreateCandidate(
        Guid candidateId,
        Guid compoundId,
        string compoundName,
        string canonicalSmiles)
    {
        return new ScreeningCandidate
        {
            Id =
                candidateId,

            ScreeningJobId =
                ScreeningJobId,

            CompoundId =
                compoundId,

            CompoundName =
                compoundName,

            CanonicalSmiles =
                canonicalSmiles,

            AddedAtUtc =
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
    }

    private static ScreeningResult CreateResult(
        Guid resultId,
        Guid compoundId,
        decimal score,
        int rank,
        bool selected,
        string notes)
    {
        return new ScreeningResult
        {
            Id =
                resultId,

            ScreeningJobId =
                ScreeningJobId,

            CompoundId =
                compoundId,

            Score =
                score,

            Rank =
                rank,

            Selected =
                selected,

            SelectedByUserId =
                selected
                    ? SystemUsers.SystemUserId
                    : null,

            SelectedAtUtc =
                selected
                    ? SeedTimestamp
                    : null,

            Notes =
                notes,

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
