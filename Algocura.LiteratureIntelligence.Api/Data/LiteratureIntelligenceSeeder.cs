using Algocura.LiteratureIntelligence.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.LiteratureIntelligence.Api.Data;

public static class LiteratureIntelligenceSeeder
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

    private static readonly Guid PublicationTrem2Id =
        Guid.Parse("10000000-0000-0000-0000-000000000001");

    private static readonly Guid PublicationApoeId =
        Guid.Parse("10000000-0000-0000-0000-000000000002");

    private static readonly Guid PublicationTrem2AuthorOneId =
        Guid.Parse("11000000-0000-0000-0000-000000000001");

    private static readonly Guid PublicationTrem2AuthorTwoId =
        Guid.Parse("11000000-0000-0000-0000-000000000002");

    private static readonly Guid PublicationApoeAuthorOneId =
        Guid.Parse("11000000-0000-0000-0000-000000000003");

    private static readonly Guid PublicationApoeAuthorTwoId =
        Guid.Parse("11000000-0000-0000-0000-000000000004");

    private static readonly Guid Trem2EvidenceId =
        Guid.Parse("12000000-0000-0000-0000-000000000001");

    private static readonly Guid ApoeEvidenceId =
        Guid.Parse("12000000-0000-0000-0000-000000000002");

    private static readonly Guid Trem2HypothesisId =
        Guid.Parse("13000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(
        LiteratureIntelligenceDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        await SeedPublicationsAsync(
            dbContext,
            cancellationToken);

        await SeedAuthorsAsync(
            dbContext,
            cancellationToken);

        await SeedEvidenceStatementsAsync(
            dbContext,
            cancellationToken);

        await SeedTargetHypothesesAsync(
            dbContext,
            cancellationToken);

        await SeedTargetHypothesisEvidenceAsync(
            dbContext,
            cancellationToken);
    }

    private static async Task SeedPublicationsAsync(
        LiteratureIntelligenceDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var seedIds =
            new[]
            {
                PublicationTrem2Id,
                PublicationApoeId
            };

        var existingIds =
            await dbContext.Publications
                .AsNoTracking()
                .Where(publication =>
                    seedIds.Contains(publication.Id))
                .Select(publication => publication.Id)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var publications =
            new[]
            {
                new Publication
                {
                    Id = PublicationTrem2Id,
                    Title =
                        "Prototype literature record: TREM2 and neurodegenerative disease",
                    Abstract =
                        "Prototype seed data describing a reported association " +
                        "between TREM2-related microglial function and " +
                        "neurodegenerative disease progression.",
                    Journal =
                        "Algocura Prototype Literature Dataset",
                    ExternalId =
                        "ALGOCURA-LIT-0001",
                    SourceUrl =
                        null,
                    PublishedDate =
                        new DateOnly(2024, 1, 15),
                    Status =
                        PublicationStatus.Analyzed,
                    CreatedAtUtc =
                        SeedTimestamp,
                    UpdatedAtUtc =
                        SeedTimestamp,
                    AnalyzedAtUtc =
                        SeedTimestamp,
                    CreatedByUserId =
                        SystemUsers.SystemUserId,
                    UpdatedByUserId =
                        SystemUsers.SystemUserId
                },
                new Publication
                {
                    Id = PublicationApoeId,
                    Title =
                        "Prototype literature record: APOE pathway and neurodegeneration",
                    Abstract =
                        "Prototype seed data describing the relationship between " +
                        "APOE-associated pathways, lipid handling, and " +
                        "neurodegenerative disease biology.",
                    Journal =
                        "Algocura Prototype Literature Dataset",
                    ExternalId =
                        "ALGOCURA-LIT-0002",
                    SourceUrl =
                        null,
                    PublishedDate =
                        new DateOnly(2024, 2, 20),
                    Status =
                        PublicationStatus.Analyzed,
                    CreatedAtUtc =
                        SeedTimestamp,
                    UpdatedAtUtc =
                        SeedTimestamp,
                    AnalyzedAtUtc =
                        SeedTimestamp,
                    CreatedByUserId =
                        SystemUsers.SystemUserId,
                    UpdatedByUserId =
                        SystemUsers.SystemUserId
                }
            };

        var missingPublications =
            publications
                .Where(publication =>
                    !existingIdSet.Contains(publication.Id))
                .ToArray();

        if (missingPublications.Length == 0)
        {
            return;
        }

        await dbContext.Publications.AddRangeAsync(
            missingPublications,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedAuthorsAsync(
        LiteratureIntelligenceDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var seedIds =
            new[]
            {
                PublicationTrem2AuthorOneId,
                PublicationTrem2AuthorTwoId,
                PublicationApoeAuthorOneId,
                PublicationApoeAuthorTwoId
            };

        var existingIds =
            await dbContext.PublicationAuthors
                .AsNoTracking()
                .Where(author =>
                    seedIds.Contains(author.Id))
                .Select(author => author.Id)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var authors =
            new[]
            {
                new PublicationAuthor
                {
                    Id =
                        PublicationTrem2AuthorOneId,
                    PublicationId =
                        PublicationTrem2Id,
                    Name =
                        "Algocura Research Team",
                    DisplayOrder =
                        1,
                    CreatedByUserId =
                        SystemUsers.SystemUserId,
                    UpdatedByUserId =
                        SystemUsers.SystemUserId
                },
                new PublicationAuthor
                {
                    Id =
                        PublicationTrem2AuthorTwoId,
                    PublicationId =
                        PublicationTrem2Id,
                    Name =
                        "Prototype Literature Curator",
                    DisplayOrder =
                        2,
                    CreatedByUserId =
                        SystemUsers.SystemUserId,
                    UpdatedByUserId =
                        SystemUsers.SystemUserId
                },
                new PublicationAuthor
                {
                    Id =
                        PublicationApoeAuthorOneId,
                    PublicationId =
                        PublicationApoeId,
                    Name =
                        "Algocura Research Team",
                    DisplayOrder =
                        1,
                    CreatedByUserId =
                        SystemUsers.SystemUserId,
                    UpdatedByUserId =
                        SystemUsers.SystemUserId
                },
                new PublicationAuthor
                {
                    Id =
                        PublicationApoeAuthorTwoId,
                    PublicationId =
                        PublicationApoeId,
                    Name =
                        "Prototype Literature Curator",
                    DisplayOrder =
                        2,
                    CreatedByUserId =
                        SystemUsers.SystemUserId,
                    UpdatedByUserId =
                        SystemUsers.SystemUserId
                }
            };

        var missingAuthors =
            authors
                .Where(author =>
                    !existingIdSet.Contains(author.Id))
                .ToArray();

        if (missingAuthors.Length == 0)
        {
            return;
        }

        await dbContext.PublicationAuthors.AddRangeAsync(
            missingAuthors,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedEvidenceStatementsAsync(
        LiteratureIntelligenceDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var seedIds =
            new[]
            {
                Trem2EvidenceId,
                ApoeEvidenceId
            };

        var existingIds =
            await dbContext.EvidenceStatements
                .AsNoTracking()
                .Where(evidence =>
                    seedIds.Contains(evidence.Id))
                .Select(evidence => evidence.Id)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var evidenceStatements =
            new[]
            {
                new EvidenceStatement
                {
                    Id =
                        Trem2EvidenceId,
                    PublicationId =
                        PublicationTrem2Id,
                    DiseaseName =
                        "Alzheimer's disease",
                    GeneSymbol =
                        "TREM2",
                    ProteinTarget =
                        "Triggering receptor expressed on myeloid cells 2",
                    RelationshipType =
                        "Disease association",
                    Statement =
                        "Prototype evidence indicates that altered TREM2 " +
                        "function may affect microglial response and disease progression.",
                    Strength =
                        EvidenceStrength.Strong,
                    ConfidenceScore =
                        0.90m,
                    HumanValidated =
                        true,
                    CreatedAtUtc =
                        SeedTimestamp,
                    CreatedByUserId =
                        SystemUsers.SystemUserId,
                    UpdatedByUserId =
                        SystemUsers.SystemUserId
                },
                new EvidenceStatement
                {
                    Id =
                        ApoeEvidenceId,
                    PublicationId =
                        PublicationApoeId,
                    DiseaseName =
                        "Alzheimer's disease",
                    GeneSymbol =
                        "APOE",
                    ProteinTarget =
                        "Apolipoprotein E",
                    RelationshipType =
                        "Pathway association",
                    Statement =
                        "Prototype evidence indicates that APOE-associated " +
                        "pathways may influence lipid transport, amyloid biology, " +
                        "and neurodegenerative disease risk.",
                    Strength =
                        EvidenceStrength.VeryStrong,
                    ConfidenceScore =
                        0.95m,
                    HumanValidated =
                        true,
                    CreatedAtUtc =
                        SeedTimestamp,
                    CreatedByUserId =
                        SystemUsers.SystemUserId,
                    UpdatedByUserId =
                        SystemUsers.SystemUserId
                }
            };

        var missingEvidenceStatements =
            evidenceStatements
                .Where(evidence =>
                    !existingIdSet.Contains(evidence.Id))
                .ToArray();

        if (missingEvidenceStatements.Length == 0)
        {
            return;
        }

        await dbContext.EvidenceStatements.AddRangeAsync(
            missingEvidenceStatements,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedTargetHypothesesAsync(
        LiteratureIntelligenceDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists =
            await dbContext.TargetHypotheses
                .AsNoTracking()
                .AnyAsync(
                    hypothesis =>
                        hypothesis.Id ==
                        Trem2HypothesisId,
                    cancellationToken);

        if (exists)
        {
            return;
        }

        var hypothesis =
            new TargetHypothesis
            {
                Id =
                    Trem2HypothesisId,
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
                EvidenceScore =
                    0.90m,
                Status =
                    TargetHypothesisStatus.Accepted,
                CreatedAtUtc =
                    SeedTimestamp,
                UpdatedAtUtc =
                    SeedTimestamp,
                CreatedByUserId =
                    SystemUsers.SystemUserId,
                UpdatedByUserId =
                    SystemUsers.SystemUserId
            };

        await dbContext.TargetHypotheses.AddAsync(
            hypothesis,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedTargetHypothesisEvidenceAsync(
        LiteratureIntelligenceDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists =
            await dbContext.TargetHypothesisEvidence
                .AsNoTracking()
                .AnyAsync(
                    link =>
                        link.TargetHypothesisId ==
                            Trem2HypothesisId &&
                        link.EvidenceStatementId ==
                            Trem2EvidenceId,
                    cancellationToken);

        if (exists)
        {
            return;
        }

        var link =
            new TargetHypothesisEvidence
            {
                TargetHypothesisId =
                    Trem2HypothesisId,
                EvidenceStatementId =
                    Trem2EvidenceId,
                CreatedByUserId =
                    SystemUsers.SystemUserId,
                UpdatedByUserId =
                    SystemUsers.SystemUserId
            };

        await dbContext.TargetHypothesisEvidence.AddAsync(
            link,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
