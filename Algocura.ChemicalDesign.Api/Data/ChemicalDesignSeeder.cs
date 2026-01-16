using Algocura.ChemicalDesign.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.ChemicalDesign.Api.Data;

public static class ChemicalDesignSeeder
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

    // Matches the selected TREM2 target assessment seeded by
    // Algocura.TargetDiscovery.Api.
    private static readonly Guid Trem2TargetAssessmentId =
        Guid.Parse("21000000-0000-0000-0000-000000000001");

    // Matches the TREM2 target hypothesis seeded by
    // Algocura.LiteratureIntelligence.Api and projected by
    // Algocura.TargetDiscovery.Api.
    private static readonly Guid Trem2TargetHypothesisId =
        Guid.Parse("13000000-0000-0000-0000-000000000001");

    private static readonly Guid DesignProjectId =
        Guid.Parse("31000000-0000-0000-0000-000000000001");

    private static readonly Guid QueryStructureId =
        Guid.Parse("32000000-0000-0000-0000-000000000001");

    private static readonly Guid StructureSearchJobId =
        Guid.Parse("33000000-0000-0000-0000-000000000001");

    private static readonly Guid CandidateAspirinId =
        Guid.Parse("34000000-0000-0000-0000-000000000001");

    private static readonly Guid CandidateNaproxenId =
        Guid.Parse("34000000-0000-0000-0000-000000000002");

    private static readonly Guid SynthesisRouteId =
        Guid.Parse("35000000-0000-0000-0000-000000000001");

    private static readonly Guid ReactionStepOneId =
        Guid.Parse("36000000-0000-0000-0000-000000000001");

    private static readonly Guid ReactionStepTwoId =
        Guid.Parse("36000000-0000-0000-0000-000000000002");

    private static readonly Guid AspirinLibraryId =
        Guid.Parse("37000000-0000-0000-0000-000000000001");

    private static readonly Guid CaffeineLibraryId =
        Guid.Parse("37000000-0000-0000-0000-000000000002");

    private static readonly Guid AcetaminophenLibraryId =
        Guid.Parse("37000000-0000-0000-0000-000000000003");

    private static readonly Guid IbuprofenLibraryId =
        Guid.Parse("37000000-0000-0000-0000-000000000004");

    private static readonly Guid NaproxenLibraryId =
        Guid.Parse("37000000-0000-0000-0000-000000000005");

    public static async Task SeedAsync(
        ChemicalDesignDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        await SeedChemicalLibraryAsync(
            dbContext,
            cancellationToken);

        await SeedSelectedTargetAsync(
            dbContext,
            cancellationToken);

        await SeedDesignProjectAsync(
            dbContext,
            cancellationToken);

        await SeedQueryStructureAsync(
            dbContext,
            cancellationToken);

        await SeedStructureSearchJobAsync(
            dbContext,
            cancellationToken);

        await SeedCandidatesAsync(
            dbContext,
            cancellationToken);

        await SeedSynthesisRouteAsync(
            dbContext,
            cancellationToken);

        await SeedReactionStepsAsync(
            dbContext,
            cancellationToken);
    }

    private static async Task SeedChemicalLibraryAsync(
        ChemicalDesignDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var entries =
            new[]
            {
                CreateLibraryEntry(
                    AspirinLibraryId,
                    "Aspirin",
                    "CC(=O)OC1=CC=CC=C1C(=O)O",
                    "BSYNRYMUTXBXSQ-UHFFFAOYSA-N",
                    "C9H8O4",
                    180.16m),

                CreateLibraryEntry(
                    CaffeineLibraryId,
                    "Caffeine",
                    "CN1C=NC2=C1C(=O)N(C(=O)N2C)C",
                    "RYYVLZVUVIJVGH-UHFFFAOYSA-N",
                    "C8H10N4O2",
                    194.19m),

                CreateLibraryEntry(
                    AcetaminophenLibraryId,
                    "Acetaminophen",
                    "CC(=O)NC1=CC=C(C=C1)O",
                    "RZVAJINKPMORJF-UHFFFAOYSA-N",
                    "C8H9NO2",
                    151.16m),

                CreateLibraryEntry(
                    IbuprofenLibraryId,
                    "Ibuprofen",
                    "CC(C)CC1=CC=C(C=C1)C(C)C(=O)O",
                    "HEFNNWSXXWATRW-UHFFFAOYSA-N",
                    "C13H18O2",
                    206.28m),

                CreateLibraryEntry(
                    NaproxenLibraryId,
                    "Naproxen",
                    "COC1=CC2=C(C=C1)C=C(C=C2)[C@@H](C)C(=O)O",
                    "CMWTZPSULFXXJA-VIFPVBQESA-N",
                    "C14H14O3",
                    230.26m)
            };

        var seedIds =
            entries
                .Select(entry => entry.Id)
                .ToArray();

        var existingIds =
            await dbContext.ChemicalLibrary
                .AsNoTracking()
                .Where(entry =>
                    seedIds.Contains(entry.Id))
                .Select(entry => entry.Id)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var missingEntries =
            entries
                .Where(entry =>
                    !existingIdSet.Contains(entry.Id))
                .ToArray();

        if (missingEntries.Length == 0)
        {
            return;
        }

        await dbContext.ChemicalLibrary.AddRangeAsync(
            missingEntries,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedSelectedTargetAsync(
        ChemicalDesignDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists =
            await dbContext.SelectedTargets
                .AsNoTracking()
                .AnyAsync(
                    target =>
                        target.TargetAssessmentId ==
                        Trem2TargetAssessmentId,
                    cancellationToken);

        if (exists)
        {
            return;
        }

        var target =
            new SelectedTargetProjection
            {
                TargetAssessmentId =
                    Trem2TargetAssessmentId,

                TargetHypothesisId =
                    Trem2TargetHypothesisId,

                DiseaseName =
                    "Alzheimer's disease",

                GeneSymbol =
                    "TREM2",

                ProteinTarget =
                    "Triggering receptor expressed on myeloid cells 2",

                DesiredModulation =
                    "Activation",

                Rationale =
                    "Prototype selected target projected from Target Discovery " +
                    "for downstream chemical-design evaluation.",

                LiteratureEvidenceScore =
                    0.90m,

                TargetAssessmentScore =
                    0.87m,

                ConfidenceLevel =
                    "VeryHigh",

                SourceUpdatedAtUtc =
                    SeedTimestamp,

                SynchronizedAtUtc =
                    SeedTimestamp,

                CreatedAtUtc =
                    SeedTimestamp,

                UpdatedAtUtc =
                    SeedTimestamp,

                CreatedByUserId =
                    SystemUsers.SystemUserId,

                UpdatedByUserId =
                    SystemUsers.SystemUserId,

                SourceCreatedByUserId =
                    SystemUsers.SystemUserId,

                SourceUpdatedByUserId =
                    SystemUsers.SystemUserId
            };

        await dbContext.SelectedTargets.AddAsync(
            target,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedDesignProjectAsync(
        ChemicalDesignDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists =
            await dbContext.DesignProjects
                .AsNoTracking()
                .AnyAsync(
                    project =>
                        project.Id ==
                        DesignProjectId,
                    cancellationToken);

        if (exists)
        {
            return;
        }

        var project =
            new ChemicalDesignProject
            {
                Id =
                    DesignProjectId,

                TargetAssessmentId =
                    Trem2TargetAssessmentId,

                Name =
                    "TREM2 chemical design project",

                Objective =
                    "Identify prototype chemical structures that could be " +
                    "evaluated for downstream TREM2-related discovery workflows.",

                DesiredModulation =
                    "Activation",

                Status =
                    DesignProjectStatus.Completed,

                CreatedAtUtc =
                    SeedTimestamp,

                UpdatedAtUtc =
                    SeedTimestamp,

                CompletedByUserId =
                    SystemUsers.SystemUserId,

                CompletedAtUtc =
                    SeedTimestamp,

                OwnerUserId =
                    SystemUsers.SystemUserId,

                CreatedByUserId =
                    SystemUsers.SystemUserId,

                UpdatedByUserId =
                    SystemUsers.SystemUserId
            };

        await dbContext.DesignProjects.AddAsync(
            project,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedQueryStructureAsync(
        ChemicalDesignDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists =
            await dbContext.QueryStructures
                .AsNoTracking()
                .AnyAsync(
                    query =>
                        query.Id ==
                        QueryStructureId,
                    cancellationToken);

        if (exists)
        {
            return;
        }

        var queryStructure =
            new QueryStructure
            {
                Id =
                    QueryStructureId,

                DesignProjectId =
                    DesignProjectId,

                Name =
                    "Prototype aromatic carboxylate pharmacophore",

                Structure =
                    "c1ccccc1C(=O)O",

                StructureFormat =
                    "SMARTS",

                SearchPurpose =
                    "Substructure search",

                CreatedAtUtc =
                    SeedTimestamp,

                UpdatedAtUtc =
                    SeedTimestamp,

                CreatedByUserId =
                    SystemUsers.SystemUserId,

                UpdatedByUserId =
                    SystemUsers.SystemUserId
            };

        await dbContext.QueryStructures.AddAsync(
            queryStructure,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedStructureSearchJobAsync(
        ChemicalDesignDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists =
            await dbContext.StructureSearchJobs
                .AsNoTracking()
                .AnyAsync(
                    job =>
                        job.Id ==
                        StructureSearchJobId,
                    cancellationToken);

        if (exists)
        {
            return;
        }

        var searchJob =
            new StructureSearchJob
            {
                Id =
                    StructureSearchJobId,

                DesignProjectId =
                    DesignProjectId,

                QueryStructureId =
                    QueryStructureId,

                SearchType =
                    StructureSearchType.Substructure,

                SimilarityThreshold =
                    0.75m,

                MaximumResults =
                    25,

                Provider =
                    "Prototype chemical library",

                Status =
                    StructureSearchStatus.Completed,

                FailureReason =
                    null,

                CreatedAtUtc =
                    SeedTimestamp,

                UpdatedAtUtc =
                    SeedTimestamp,

                StartedAtUtc =
                    SeedTimestamp,

                CompletedByUserId =
                    SystemUsers.SystemUserId,

                CompletedAtUtc =
                    SeedTimestamp,

                CreatedByUserId =
                    SystemUsers.SystemUserId,

                UpdatedByUserId =
                    SystemUsers.SystemUserId
            };

        await dbContext.StructureSearchJobs.AddAsync(
            searchJob,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedCandidatesAsync(
        ChemicalDesignDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var candidates =
            new[]
            {
                new CandidateStructure
                {
                    Id =
                        CandidateAspirinId,

                    DesignProjectId =
                        DesignProjectId,

                    StructureSearchJobId =
                        StructureSearchJobId,

                    Name =
                        "Aspirin",

                    CanonicalSmiles =
                        "CC(=O)OC1=CC=CC=C1C(=O)O",

                    InchiKey =
                        "BSYNRYMUTXBXSQ-UHFFFAOYSA-N",

                    MolecularFormula =
                        "C9H8O4",

                    MolecularWeight =
                        180.16m,

                    MatchScore =
                        0.91m,

                    SourceReference =
                        "Prototype chemical library",

                    SelectionRationale =
                        "Selected as the preferred prototype candidate " +
                        "for demonstrating downstream compound registration.",

                    Status =
                        CandidateStructureStatus.SelectedForRegistration,

                    CreatedAtUtc =
                        SeedTimestamp,

                    UpdatedAtUtc =
                        SeedTimestamp,

                    SelectedByUserId =
                        SystemUsers.SystemUserId,

                    SelectedAtUtc =
                        SeedTimestamp,

                    CreatedByUserId =
                        SystemUsers.SystemUserId,

                    UpdatedByUserId =
                        SystemUsers.SystemUserId
                },

                new CandidateStructure
                {
                    Id =
                        CandidateNaproxenId,

                    DesignProjectId =
                        DesignProjectId,

                    StructureSearchJobId =
                        StructureSearchJobId,

                    Name =
                        "Naproxen",

                    CanonicalSmiles =
                        "COC1=CC2=C(C=C1)C=C(C=C2)[C@@H](C)C(=O)O",

                    InchiKey =
                        "CMWTZPSULFXXJA-VIFPVBQESA-N",

                    MolecularFormula =
                        "C14H14O3",

                    MolecularWeight =
                        230.26m,

                    MatchScore =
                        0.82m,

                    SourceReference =
                        "Prototype chemical library",

                    SelectionRationale =
                        "Retained as a secondary prototype candidate.",

                    Status =
                        CandidateStructureStatus.UnderReview,

                    CreatedAtUtc =
                        SeedTimestamp,

                    UpdatedAtUtc =
                        SeedTimestamp,

                    SelectedByUserId =
                        null,

                    SelectedAtUtc =
                        null,

                    CreatedByUserId =
                        SystemUsers.SystemUserId,

                    UpdatedByUserId =
                        SystemUsers.SystemUserId
                }
            };

        var seedIds =
            candidates
                .Select(candidate => candidate.Id)
                .ToArray();

        var existingIds =
            await dbContext.CandidateStructures
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

        await dbContext.CandidateStructures.AddRangeAsync(
            missingCandidates,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedSynthesisRouteAsync(
        ChemicalDesignDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists =
            await dbContext.SynthesisRoutes
                .AsNoTracking()
                .AnyAsync(
                    route =>
                        route.Id ==
                        SynthesisRouteId,
                    cancellationToken);

        if (exists)
        {
            return;
        }

        var route =
            new SynthesisRoute
            {
                Id =
                    SynthesisRouteId,

                CandidateStructureId =
                    CandidateAspirinId,

                Name =
                    "Prototype aspirin synthesis route",

                Description =
                    "Two-step prototype route included for architecture " +
                    "and workflow demonstration only.",

                EstimatedYieldPercent =
                    78.50m,

                StepCount =
                    2,

                Status =
                    SynthesisRouteStatus.Preferred,

                CreatedAtUtc =
                    SeedTimestamp,

                UpdatedAtUtc =
                    SeedTimestamp,

                ApprovedByUserId =
                    SystemUsers.SystemUserId,

                ApprovedAtUtc =
                    SeedTimestamp,

                CreatedByUserId =
                    SystemUsers.SystemUserId,

                UpdatedByUserId =
                    SystemUsers.SystemUserId
            };

        await dbContext.SynthesisRoutes.AddAsync(
            route,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedReactionStepsAsync(
        ChemicalDesignDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var steps =
            new[]
            {
                new ReactionStep
                {
                    Id =
                        ReactionStepOneId,

                    SynthesisRouteId =
                        SynthesisRouteId,

                    StepNumber =
                        1,

                    ReactionName =
                        "Salicylic acid preparation",

                    Reactants =
                        "Prototype aromatic precursor",

                    Reagents =
                        "Prototype hydrolysis reagents",

                    Conditions =
                        "Prototype controlled aqueous conditions",

                    ProductSmiles =
                        "O=C(O)C1=CC=CC=C1O",

                    EstimatedYieldPercent =
                        85.00m,

                    CreatedAtUtc =
                        SeedTimestamp,

                    UpdatedAtUtc =
                        SeedTimestamp,

                    CreatedByUserId =
                        SystemUsers.SystemUserId,

                    UpdatedByUserId =
                        SystemUsers.SystemUserId
                },

                new ReactionStep
                {
                    Id =
                        ReactionStepTwoId,

                    SynthesisRouteId =
                        SynthesisRouteId,

                    StepNumber =
                        2,

                    ReactionName =
                        "Acetylation",

                    Reactants =
                        "Salicylic acid",

                    Reagents =
                        "Prototype acetylating reagent",

                    Conditions =
                        "Prototype controlled temperature conditions",

                    ProductSmiles =
                        "CC(=O)OC1=CC=CC=C1C(=O)O",

                    EstimatedYieldPercent =
                        92.00m,

                    CreatedAtUtc =
                        SeedTimestamp,

                    UpdatedAtUtc =
                        SeedTimestamp,

                    CreatedByUserId =
                        SystemUsers.SystemUserId,

                    UpdatedByUserId =
                        SystemUsers.SystemUserId
                }
            };

        var seedIds =
            steps
                .Select(step => step.Id)
                .ToArray();

        var existingIds =
            await dbContext.ReactionSteps
                .AsNoTracking()
                .Where(step =>
                    seedIds.Contains(step.Id))
                .Select(step => step.Id)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var missingSteps =
            steps
                .Where(step =>
                    !existingIdSet.Contains(step.Id))
                .ToArray();

        if (missingSteps.Length == 0)
        {
            return;
        }

        await dbContext.ReactionSteps.AddRangeAsync(
            missingSteps,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static ChemicalLibraryEntry CreateLibraryEntry(
        Guid id,
        string name,
        string smiles,
        string inchiKey,
        string molecularFormula,
        decimal molecularWeight)
    {
        return new ChemicalLibraryEntry
        {
            Id =
                id,

            Name =
                name,

            CanonicalSmiles =
                smiles,

            InchiKey =
                inchiKey,

            MolecularFormula =
                molecularFormula,

            MolecularWeight =
                molecularWeight,

            SourceReference =
                "Prototype chemical library",

            CreatedAtUtc =
                SeedTimestamp,

            UpdatedAtUtc =
                SeedTimestamp,

            CreatedByUserId =
                SystemUsers.SystemUserId,

            UpdatedByUserId =
                SystemUsers.SystemUserId
        };
    }
}
