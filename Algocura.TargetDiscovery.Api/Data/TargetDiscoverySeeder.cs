using Algocura.TargetDiscovery.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.TargetDiscovery.Api.Data;

public static class TargetDiscoverySeeder
{
    private static readonly DateTimeOffset SeedTimestamp =
        new(
            2026,
            1,
            11,
            0,
            0,
            0,
            TimeSpan.Zero);

    // Matches the TREM2 target hypothesis seeded by
    // Algocura.LiteratureIntelligence.Api.
    private static readonly Guid Trem2TargetHypothesisId =
        Guid.Parse("13000000-0000-0000-0000-000000000001");

    // Matches the TREM2 evidence statement seeded by
    // Algocura.LiteratureIntelligence.Api.
    private static readonly Guid Trem2EvidenceStatementId =
        Guid.Parse("12000000-0000-0000-0000-000000000001");

    private static readonly Guid Trem2TargetAssessmentId =
        Guid.Parse("21000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(
        TargetDiscoveryDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        await SeedTargetHypothesesAsync(
            dbContext,
            cancellationToken);

        await SeedTargetHypothesisEvidenceAsync(
            dbContext,
            cancellationToken);

        await SeedTargetAssessmentsAsync(
            dbContext,
            cancellationToken);
    }

    private static async Task SeedTargetHypothesesAsync(
        TargetDiscoveryDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists =
            await dbContext.TargetHypotheses
                .AsNoTracking()
                .AnyAsync(
                    hypothesis =>
                        hypothesis.TargetHypothesisId ==
                        Trem2TargetHypothesisId,
                    cancellationToken);

        if (exists)
        {
            return;
        }

        var hypothesis =
            new TargetHypothesisProjection
            {
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
                    "Prototype hypothesis proposing that enhancing TREM2 " +
                    "signaling could support a beneficial microglial response.",

                LiteratureEvidenceScore =
                    0.90m,

                Status =
                    TargetHypothesisProjectionStatus.Accepted,

                SourceUpdatedAtUtc =
                    SeedTimestamp,

                SynchronizedAtUtc =
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

        await dbContext.TargetHypotheses.AddAsync(
            hypothesis,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedTargetHypothesisEvidenceAsync(
        TargetDiscoveryDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists =
            await dbContext.TargetHypothesisEvidence
                .AsNoTracking()
                .AnyAsync(
                    link =>
                        link.TargetHypothesisId ==
                            Trem2TargetHypothesisId &&
                        link.EvidenceStatementId ==
                            Trem2EvidenceStatementId,
                    cancellationToken);

        if (exists)
        {
            return;
        }

        var evidenceLink =
            new TargetHypothesisEvidenceProjection
            {
                TargetHypothesisId =
                    Trem2TargetHypothesisId,

                EvidenceStatementId =
                    Trem2EvidenceStatementId,

                CreatedByUserId =
                    SystemUsers.SystemUserId,

                UpdatedByUserId =
                    SystemUsers.SystemUserId,

                CreatedAtUtc =
                    SeedTimestamp,

                UpdatedAtUtc =
                    SeedTimestamp
            };

        await dbContext.TargetHypothesisEvidence.AddAsync(
            evidenceLink,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedTargetAssessmentsAsync(
        TargetDiscoveryDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists =
            await dbContext.TargetAssessments
                .AsNoTracking()
                .AnyAsync(
                    assessment =>
                        assessment.Id ==
                        Trem2TargetAssessmentId,
                    cancellationToken);

        if (exists)
        {
            return;
        }

        var assessment =
            new TargetAssessment
            {
                Id =
                    Trem2TargetAssessmentId,

                TargetHypothesisId =
                    Trem2TargetHypothesisId,

                Name =
                    "TREM2 target assessment",

                ScientificRationale =
                    "Prototype assessment combining literature evidence, " +
                    "genetic evidence, biological plausibility, druggability, " +
                    "safety considerations, and translational potential.",

                LiteratureEvidenceScore =
                    0.90m,

                GeneticEvidenceScore =
                    0.93m,

                BiologicalPlausibilityScore =
                    0.91m,

                DruggabilityScore =
                    0.82m,

                SafetyRiskScore =
                    0.72m,

                TranslationalPotentialScore =
                    0.88m,

                OverallScore =
                    0.87m,

                ConfidenceLevel =
                    TargetConfidenceLevel.VeryHigh,

                Status =
                    TargetAssessmentStatus.Selected,

                ReviewNotes =
                    "Prototype seed assessment selected for downstream " +
                    "chemical-design processing.",

                CreatedAtUtc =
                    SeedTimestamp,

                UpdatedAtUtc =
                    SeedTimestamp,

                CompletedAtUtc =
                    SeedTimestamp,

                SelectedAtUtc =
                    SeedTimestamp,

                CreatedByUserId =
                    SystemUsers.SystemUserId,

                UpdatedByUserId =
                    SystemUsers.SystemUserId
            };

        await dbContext.TargetAssessments.AddAsync(
            assessment,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
