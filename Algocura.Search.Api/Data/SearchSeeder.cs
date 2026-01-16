using Algocura.Search.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Search.Api.Data;

public static class SearchSeeder
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

    private static readonly Guid PublicationDocumentId =
        Guid.Parse("91000000-0000-0000-0000-000000000001");

    private static readonly Guid TargetDocumentId =
        Guid.Parse("91000000-0000-0000-0000-000000000002");

    private static readonly Guid ChemicalDesignDocumentId =
        Guid.Parse("91000000-0000-0000-0000-000000000003");

    private static readonly Guid CompoundDocumentId =
        Guid.Parse("91000000-0000-0000-0000-000000000004");

    private static readonly Guid ScreeningDocumentId =
        Guid.Parse("91000000-0000-0000-0000-000000000005");

    private static readonly Guid AssayDocumentId =
        Guid.Parse("91000000-0000-0000-0000-000000000006");

    private static readonly Guid PredictionDocumentId =
        Guid.Parse("91000000-0000-0000-0000-000000000007");

    private static readonly Guid WorkflowDocumentId =
        Guid.Parse("91000000-0000-0000-0000-000000000008");

    private static readonly Guid NotificationDocumentId =
        Guid.Parse("91000000-0000-0000-0000-000000000009");

    private static readonly Guid PublicationTagId =
        Guid.Parse("92000000-0000-0000-0000-000000000001");

    private static readonly Guid TargetTagId =
        Guid.Parse("92000000-0000-0000-0000-000000000002");

    private static readonly Guid ChemicalDesignTagId =
        Guid.Parse("92000000-0000-0000-0000-000000000003");

    private static readonly Guid CompoundTagId =
        Guid.Parse("92000000-0000-0000-0000-000000000004");

    private static readonly Guid ScreeningTagId =
        Guid.Parse("92000000-0000-0000-0000-000000000005");

    private static readonly Guid AssayTagId =
        Guid.Parse("92000000-0000-0000-0000-000000000006");

    private static readonly Guid PredictionTagId =
        Guid.Parse("92000000-0000-0000-0000-000000000007");

    private static readonly Guid WorkflowTagId =
        Guid.Parse("92000000-0000-0000-0000-000000000008");

    private static readonly Guid NotificationTagId =
        Guid.Parse("92000000-0000-0000-0000-000000000009");

    private static readonly Guid PublicationSourceEntityId =
        Guid.Parse("10000000-0000-0000-0000-000000000001");

    private static readonly Guid TargetSourceEntityId =
        Guid.Parse("21000000-0000-0000-0000-000000000001");

    private static readonly Guid ChemicalDesignSourceEntityId =
        Guid.Parse("31000000-0000-0000-0000-000000000001");

    private static readonly Guid CompoundSourceEntityId =
        Guid.Parse("7f06dc5b-379f-4589-9384-4f09c55f97c3");

    private static readonly Guid ScreeningSourceEntityId =
        Guid.Parse("41000000-0000-0000-0000-000000000001");

    private static readonly Guid AssaySourceEntityId =
        Guid.Parse("62000000-0000-0000-0000-000000000001");

    private static readonly Guid PredictionSourceEntityId =
        Guid.Parse("52000000-0000-0000-0000-000000000001");

    private static readonly Guid WorkflowSourceEntityId =
        Guid.Parse("71000000-0000-0000-0000-000000000001");

    private static readonly Guid NotificationSourceEntityId =
        Guid.Parse("82000000-0000-0000-0000-000000000003");

    public static async Task SeedAsync(
        SearchDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        await SeedDocumentsAsync(
            dbContext,
            cancellationToken);

        await SeedTagsAsync(
            dbContext,
            cancellationToken);
    }

    private static async Task SeedDocumentsAsync(
        SearchDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var documents =
            new[]
            {
                CreateDocument(
                    PublicationDocumentId,
                    PublicationSourceEntityId,
                    "LiteratureIntelligenceSnapshot",
                    "Algocura.LiteratureIntelligence.Api",
                    SearchDocumentType.Publication,
                    "TREM2 and neurodegenerative disease",
                    "Prototype literature record describing TREM2-related " +
                    "microglial biology and neurodegenerative disease progression.",
                    "ALGOCURA-LIT-0001",
                    "Alzheimer's disease",
                    "TREM2",
                    null,
                    null,
                    null,
                    "Analyzed",
                    0.90m),

                CreateDocument(
                    TargetDocumentId,
                    TargetSourceEntityId,
                    "TargetDiscoverySnapshot",
                    "Algocura.TargetDiscovery.Api",
                    SearchDocumentType.Target,
                    "TREM2 target assessment",
                    "Selected TREM2 target assessment with very high confidence.",
                    null,
                    "Alzheimer's disease",
                    "TREM2",
                    null,
                    null,
                    null,
                    "Selected",
                    0.87m),

                CreateDocument(
                    ChemicalDesignDocumentId,
                    ChemicalDesignSourceEntityId,
                    "ChemicalDesignSnapshot",
                    "Algocura.ChemicalDesign.Api",
                    SearchDocumentType.ChemicalDesign,
                    "TREM2 chemical design project",
                    "Completed chemical-design project containing prototype " +
                    "query structures and selected candidates.",
                    null,
                    "Alzheimer's disease",
                    "TREM2",
                    "Aspirin",
                    "BSYNRYMUTXBXSQ-UHFFFAOYSA-N",
                    "CC(=O)OC1=CC=CC=C1C(=O)O",
                    "Completed",
                    0.91m),

                CreateDocument(
                    CompoundDocumentId,
                    CompoundSourceEntityId,
                    "CompoundCatalogSnapshot",
                    "Algocura.CompoundCatalog.Api",
                    SearchDocumentType.Compound,
                    "Aspirin",
                    "Registered prototype compound available for screening.",
                    "BSYNRYMUTXBXSQ-UHFFFAOYSA-N",
                    null,
                    null,
                    "Aspirin",
                    "BSYNRYMUTXBXSQ-UHFFFAOYSA-N",
                    "CC(=O)OC1=CC=CC=C1C(=O)O",
                    "Registered",
                    null),

                CreateDocument(
                    ScreeningDocumentId,
                    ScreeningSourceEntityId,
                    "CompletedScreeningSnapshot",
                    "Algocura.Screening.Api",
                    SearchDocumentType.Screening,
                    "Prototype TREM2 compound screening",
                    "Completed screening with Aspirin and Naproxen selected " +
                    "for downstream assay processing.",
                    null,
                    "Alzheimer's disease",
                    "TREM2",
                    null,
                    null,
                    null,
                    "Completed",
                    0.93m),

                CreateDocument(
                    AssayDocumentId,
                    AssaySourceEntityId,
                    "AssaySnapshot",
                    "Algocura.AssayManagement.Api",
                    SearchDocumentType.Assay,
                    "Prototype TREM2 functional activity assay",
                    "Published functional assay with passed quality control.",
                    null,
                    "Alzheimer's disease",
                    "TREM2",
                    "Aspirin",
                    null,
                    null,
                    "Published",
                    0.94m),

                CreateDocument(
                    PredictionDocumentId,
                    PredictionSourceEntityId,
                    "PredictionJobCompleted",
                    "Algocura.AIDiscovery.Api",
                    SearchDocumentType.Prediction,
                    "Prototype TREM2 candidate prioritization",
                    "Completed prediction job ranking Aspirin and Naproxen " +
                    "as prioritized candidates.",
                    null,
                    "Alzheimer's disease",
                    "TREM2",
                    "Aspirin",
                    null,
                    null,
                    "Completed",
                    0.96m),

                CreateDocument(
                    WorkflowDocumentId,
                    WorkflowSourceEntityId,
                    "WorkflowSnapshot",
                    "Algocura.Workflow.Api",
                    SearchDocumentType.Workflow,
                    "Prototype TREM2 discovery workflow",
                    "Completed end-to-end discovery workflow from literature " +
                    "intelligence through AI candidate prioritization.",
                    null,
                    "Alzheimer's disease",
                    "TREM2",
                    null,
                    null,
                    null,
                    "Completed",
                    null),

                CreateDocument(
                    NotificationDocumentId,
                    NotificationSourceEntityId,
                    "NotificationSnapshot",
                    "Algocura.Notification.Api",
                    SearchDocumentType.Notification,
                    "Discovery workflow completed",
                    "Delivered in-application notification for the completed " +
                    "prototype TREM2 workflow.",
                    null,
                    "Alzheimer's disease",
                    "TREM2",
                    null,
                    null,
                    null,
                    "Delivered",
                    null)
            };

        var seedIds =
            documents
                .Select(document => document.Id)
                .ToArray();

        var existingIds =
            await dbContext.SearchDocuments
                .AsNoTracking()
                .Where(document =>
                    seedIds.Contains(document.Id))
                .Select(document => document.Id)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var missingDocuments =
            documents
                .Where(document =>
                    !existingIdSet.Contains(document.Id))
                .ToArray();

        if (missingDocuments.Length == 0)
        {
            return;
        }

        await dbContext.SearchDocuments.AddRangeAsync(
            missingDocuments,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedTagsAsync(
        SearchDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var tags =
            new[]
            {
                CreateTag(
                    PublicationTagId,
                    PublicationDocumentId,
                    "stage",
                    "literature-intelligence"),

                CreateTag(
                    TargetTagId,
                    TargetDocumentId,
                    "stage",
                    "target-discovery"),

                CreateTag(
                    ChemicalDesignTagId,
                    ChemicalDesignDocumentId,
                    "stage",
                    "chemical-design"),

                CreateTag(
                    CompoundTagId,
                    CompoundDocumentId,
                    "stage",
                    "compound-catalog"),

                CreateTag(
                    ScreeningTagId,
                    ScreeningDocumentId,
                    "stage",
                    "screening"),

                CreateTag(
                    AssayTagId,
                    AssayDocumentId,
                    "stage",
                    "assay-management"),

                CreateTag(
                    PredictionTagId,
                    PredictionDocumentId,
                    "stage",
                    "ai-discovery"),

                CreateTag(
                    WorkflowTagId,
                    WorkflowDocumentId,
                    "stage",
                    "workflow"),

                CreateTag(
                    NotificationTagId,
                    NotificationDocumentId,
                    "stage",
                    "notification")
            };

        var seedIds =
            tags
                .Select(tag => tag.Id)
                .ToArray();

        var existingIds =
            await dbContext.SearchDocumentTags
                .AsNoTracking()
                .Where(tag =>
                    seedIds.Contains(tag.Id))
                .Select(tag => tag.Id)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var missingTags =
            tags
                .Where(tag =>
                    !existingIdSet.Contains(tag.Id))
                .ToArray();

        if (missingTags.Length == 0)
        {
            return;
        }

        await dbContext.SearchDocumentTags.AddRangeAsync(
            missingTags,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static SearchDocument CreateDocument(
        Guid id,
        Guid sourceEntityId,
        string sourceEventType,
        string sourceService,
        SearchDocumentType documentType,
        string title,
        string summary,
        string? externalReference,
        string? diseaseName,
        string? geneSymbol,
        string? compoundName,
        string? inchiKey,
        string? canonicalSmiles,
        string? outcome,
        decimal? score)
    {
        return new SearchDocument
        {
            Id =
                id,

            SourceEntityId =
                sourceEntityId,

            SourceEventType =
                sourceEventType,

            SourceService =
                sourceService,

            DocumentType =
                documentType,

            Title =
                title,

            Summary =
                summary,

            ExternalReference =
                externalReference,

            CorrelationId =
                "prototype-trem2-correlation",

            WorkflowId =
                WorkflowSourceEntityId.ToString(),

            DiseaseName =
                diseaseName,

            GeneSymbol =
                geneSymbol,

            CompoundName =
                compoundName,

            InchiKey =
                inchiKey,

            CanonicalSmiles =
                canonicalSmiles,

            Outcome =
                outcome,

            Score =
                score,

            Status =
                SearchDocumentStatus.Active,

            SourceUpdatedAtUtc =
                SeedTimestamp,

            IndexedAtUtc =
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

    private static SearchDocumentTag CreateTag(
        Guid id,
        Guid searchDocumentId,
        string name,
        string value)
    {
        return new SearchDocumentTag
        {
            Id =
                id,

            SearchDocumentId =
                searchDocumentId,

            Name =
                name,

            Value =
                value,

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
