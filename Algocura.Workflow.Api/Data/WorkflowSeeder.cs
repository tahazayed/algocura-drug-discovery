using Algocura.Workflow.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Workflow.Api.Data;

public static class WorkflowSeeder
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

    private static readonly Guid WorkflowId =
        Guid.Parse("71000000-0000-0000-0000-000000000001");

    private static readonly Guid LiteratureStageId =
        Guid.Parse("72000000-0000-0000-0000-000000000001");

    private static readonly Guid TargetStageId =
        Guid.Parse("72000000-0000-0000-0000-000000000002");

    private static readonly Guid ChemicalDesignStageId =
        Guid.Parse("72000000-0000-0000-0000-000000000003");

    private static readonly Guid CompoundCatalogStageId =
        Guid.Parse("72000000-0000-0000-0000-000000000004");

    private static readonly Guid ScreeningStageId =
        Guid.Parse("72000000-0000-0000-0000-000000000005");

    private static readonly Guid AssayStageId =
        Guid.Parse("72000000-0000-0000-0000-000000000006");

    private static readonly Guid AiDiscoveryStageId =
        Guid.Parse("72000000-0000-0000-0000-000000000007");

    private static readonly Guid LiteratureEventId =
        Guid.Parse("73000000-0000-0000-0000-000000000001");

    private static readonly Guid TargetEventId =
        Guid.Parse("73000000-0000-0000-0000-000000000002");

    private static readonly Guid ChemicalDesignEventId =
        Guid.Parse("73000000-0000-0000-0000-000000000003");

    private static readonly Guid CompoundCatalogEventId =
        Guid.Parse("73000000-0000-0000-0000-000000000004");

    private static readonly Guid ScreeningEventId =
        Guid.Parse("73000000-0000-0000-0000-000000000005");

    private static readonly Guid AssayEventId =
        Guid.Parse("73000000-0000-0000-0000-000000000006");

    private static readonly Guid AiDiscoveryEventId =
        Guid.Parse("73000000-0000-0000-0000-000000000007");

    // IDs aligned with the seeded upstream services.
    private static readonly Guid LiteratureHypothesisId =
        Guid.Parse("13000000-0000-0000-0000-000000000001");

    private static readonly Guid TargetAssessmentId =
        Guid.Parse("21000000-0000-0000-0000-000000000001");

    private static readonly Guid ChemicalDesignProjectId =
        Guid.Parse("31000000-0000-0000-0000-000000000001");

    private static readonly Guid CompoundId =
        Guid.Parse("7f06dc5b-379f-4589-9384-4f09c55f97c3");

    private static readonly Guid ScreeningId =
        Guid.Parse("41000000-0000-0000-0000-000000000001");

    private static readonly Guid AssayId =
        Guid.Parse("62000000-0000-0000-0000-000000000001");

    private static readonly Guid PredictionJobId =
        Guid.Parse("52000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(
        WorkflowDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        await SeedWorkflowAsync(
            dbContext,
            cancellationToken);

        await SeedStagesAsync(
            dbContext,
            cancellationToken);

        await SeedEventReceiptsAsync(
            dbContext,
            cancellationToken);
    }

    private static async Task SeedWorkflowAsync(
        WorkflowDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists =
            await dbContext.Workflows
                .AsNoTracking()
                .AnyAsync(
                    workflow =>
                        workflow.Id ==
                        WorkflowId,
                    cancellationToken);

        if (exists)
        {
            return;
        }

        var workflow =
            new DiscoveryWorkflow
            {
                Id =
                    WorkflowId,

                Name =
                    "Prototype TREM2 discovery workflow",

                DiseaseName =
                    "Alzheimer's disease",

                GeneSymbol =
                    "TREM2",

                Objective =
                    "Demonstrate the complete Algocura drug-discovery workflow " +
                    "from literature intelligence through AI candidate prioritization.",

                Status =
                    DiscoveryWorkflowStatus.Completed,

                CurrentStage =
                    WorkflowStage.AiDiscovery,

                OwnerUserId =
                    SystemUsers.SystemUserId,

                StartedByUserId =
                    SystemUsers.SystemUserId,

                StartedAtUtc =
                    SeedTimestamp,

                CompletedByUserId =
                    SystemUsers.SystemUserId,

                CompletedAtUtc =
                    SeedTimestamp,

                CancelledByUserId =
                    null,

                CancelledAtUtc =
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

        await dbContext.Workflows.AddAsync(
            workflow,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedStagesAsync(
        WorkflowDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var stages =
            new[]
            {
                CreateStage(
                    LiteratureStageId,
                    WorkflowStage.LiteratureIntelligence,
                    LiteratureHypothesisId,
                    "TargetHypothesis",
                    "Literature evidence produced and target hypothesis accepted."),

                CreateStage(
                    TargetStageId,
                    WorkflowStage.TargetDiscovery,
                    TargetAssessmentId,
                    "TargetAssessment",
                    "TREM2 target assessment completed and selected."),

                CreateStage(
                    ChemicalDesignStageId,
                    WorkflowStage.ChemicalDesign,
                    ChemicalDesignProjectId,
                    "ChemicalDesignProject",
                    "Chemical design project completed with selected candidate structures."),

                CreateStage(
                    CompoundCatalogStageId,
                    WorkflowStage.CompoundCatalog,
                    CompoundId,
                    "Compound",
                    "Selected compound registered in the compound catalog."),

                CreateStage(
                    ScreeningStageId,
                    WorkflowStage.Screening,
                    ScreeningId,
                    "ScreeningJob",
                    "Prototype screening completed with ranked compound results."),

                CreateStage(
                    AssayStageId,
                    WorkflowStage.AssayManagement,
                    AssayId,
                    "Assay",
                    "Functional assay completed, approved, and published."),

                CreateStage(
                    AiDiscoveryStageId,
                    WorkflowStage.AiDiscovery,
                    PredictionJobId,
                    "PredictionJob",
                    "AI Discovery prioritization completed.")
            };

        var seedIds =
            stages
                .Select(stage => stage.Id)
                .ToArray();

        var existingIds =
            await dbContext.WorkflowStages
                .AsNoTracking()
                .Where(stage =>
                    seedIds.Contains(stage.Id))
                .Select(stage => stage.Id)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var missingStages =
            stages
                .Where(stage =>
                    !existingIdSet.Contains(stage.Id))
                .ToArray();

        if (missingStages.Length == 0)
        {
            return;
        }

        await dbContext.WorkflowStages.AddRangeAsync(
            missingStages,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedEventReceiptsAsync(
        WorkflowDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var receipts =
            new[]
            {
                CreateReceipt(
                    LiteratureEventId,
                    "LiteratureIntelligenceSnapshot",
                    "Algocura.LiteratureIntelligence.Api"),

                CreateReceipt(
                    TargetEventId,
                    "TargetDiscoverySnapshot",
                    "Algocura.TargetDiscovery.Api"),

                CreateReceipt(
                    ChemicalDesignEventId,
                    "ChemicalDesignSnapshot",
                    "Algocura.ChemicalDesign.Api"),

                CreateReceipt(
                    CompoundCatalogEventId,
                    "CompoundCatalogSnapshot",
                    "Algocura.CompoundCatalog.Api"),

                CreateReceipt(
                    ScreeningEventId,
                    "CompletedScreeningSnapshot",
                    "Algocura.Screening.Api"),

                CreateReceipt(
                    AssayEventId,
                    "AssaySnapshot",
                    "Algocura.AssayManagement.Api"),

                CreateReceipt(
                    AiDiscoveryEventId,
                    "PredictionJobCompleted",
                    "Algocura.AIDiscovery.Api")
            };

        var seedIds =
            receipts
                .Select(receipt => receipt.EventId)
                .ToArray();

        var existingIds =
            await dbContext.WorkflowEventReceipts
                .AsNoTracking()
                .Where(receipt =>
                    seedIds.Contains(receipt.EventId))
                .Select(receipt => receipt.EventId)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var missingReceipts =
            receipts
                .Where(receipt =>
                    !existingIdSet.Contains(receipt.EventId))
                .ToArray();

        if (missingReceipts.Length == 0)
        {
            return;
        }

        await dbContext.WorkflowEventReceipts.AddRangeAsync(
            missingReceipts,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static WorkflowStageExecution CreateStage(
        Guid id,
        WorkflowStage stage,
        Guid externalEntityId,
        string externalEntityType,
        string summary)
    {
        return new WorkflowStageExecution
        {
            Id =
                id,

            WorkflowId =
                WorkflowId,

            Stage =
                stage,

            Status =
                WorkflowStageStatus.Completed,

            ExternalEntityId =
                externalEntityId,

            ExternalEntityType =
                externalEntityType,

            Summary =
                summary,

            FailureReason =
                null,

            StartedByUserId =
                SystemUsers.SystemUserId,

            StartedAtUtc =
                SeedTimestamp,

            CompletedByUserId =
                SystemUsers.SystemUserId,

            CompletedAtUtc =
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

    private static WorkflowEventReceipt CreateReceipt(
        Guid eventId,
        string eventType,
        string source)
    {
        return new WorkflowEventReceipt
        {
            EventId =
                eventId,

            WorkflowId =
                WorkflowId,

            EventType =
                eventType,

            Source =
                source,

            OccurredAtUtc =
                SeedTimestamp,

            ProcessedAtUtc =
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
}
