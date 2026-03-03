using Algocura.Reporting.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Reporting.Api.Data;

public static class ReportingSeeder
{
    private static readonly DateTimeOffset SeedTimestamp =
        new(2026, 1, 16, 0, 0, 0, TimeSpan.Zero);

    public static readonly Guid PlatformSummaryDefinitionId =
        Guid.Parse("a1000000-0000-0000-0000-000000000001");

    public static readonly Guid DiscoveryFunnelDefinitionId =
        Guid.Parse("a1000000-0000-0000-0000-000000000002");

    private static readonly Guid PlatformSummaryRunId =
        Guid.Parse("a2000000-0000-0000-0000-000000000001");

    private static readonly Guid DiscoveryFunnelRunId =
        Guid.Parse("a2000000-0000-0000-0000-000000000002");

    public static async Task SeedAsync(
        ReportingDbContext db,
        CancellationToken cancellationToken = default)
    {
        await SeedDefinitionsAsync(db, cancellationToken);
        await SeedProjectionsAsync(db, cancellationToken);
        await SeedRunsAsync(db, cancellationToken);
        await SeedMetricsAsync(db, cancellationToken);
    }

    private static async Task SeedDefinitionsAsync(
        ReportingDbContext db,
        CancellationToken cancellationToken)
    {
        var definitions = new[]
        {
            Definition(
                PlatformSummaryDefinitionId,
                "Platform Summary",
                "PlatformSummary",
                "Summarizes reporting projections by platform stage.",
                "{}"),
            Definition(
                DiscoveryFunnelDefinitionId,
                "Discovery Funnel",
                "DiscoveryFunnel",
                "Summarizes progression from literature through AI prioritization.",
                "{\"diseaseName\":\"Alzheimer's disease\",\"geneSymbol\":\"TREM2\"}")
        };

        var ids = definitions.Select(x => x.Id).ToArray();
        var existing = await db.ReportDefinitions.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = definitions
            .Where(x => !existing.Contains(x.Id))
            .ToArray();

        if (missing.Length > 0)
        {
            await db.ReportDefinitions.AddRangeAsync(missing, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task SeedProjectionsAsync(
        ReportingDbContext db,
        CancellationToken cancellationToken)
    {
        var workflowId =
            Guid.Parse("71000000-0000-0000-0000-000000000001")
                .ToString();

        var projections = new[]
        {
            Projection("a3000000-0000-0000-0000-000000000001", "10000000-0000-0000-0000-000000000001", ReportingProjectionType.Literature, "LiteratureIntelligenceSnapshot", "Algocura.LiteratureIntelligence.Api", "TREM2 literature evidence", "Alzheimer's disease", "TREM2", null, "Analyzed", 0.90m, workflowId),
            Projection("a3000000-0000-0000-0000-000000000002", "21000000-0000-0000-0000-000000000001", ReportingProjectionType.Target, "TargetDiscoverySnapshot", "Algocura.TargetDiscovery.Api", "TREM2 target assessment", "Alzheimer's disease", "TREM2", null, "Selected", 0.87m, workflowId),
            Projection("a3000000-0000-0000-0000-000000000003", "31000000-0000-0000-0000-000000000001", ReportingProjectionType.ChemicalDesign, "ChemicalDesignSnapshot", "Algocura.ChemicalDesign.Api", "TREM2 chemical design project", "Alzheimer's disease", "TREM2", "Aspirin", "Completed", 0.91m, workflowId),
            Projection("a3000000-0000-0000-0000-000000000004", "7f06dc5b-379f-4589-9384-4f09c55f97c3", ReportingProjectionType.Compound, "CompoundCatalogSnapshot", "Algocura.CompoundCatalog.Api", "Aspirin", null, null, "Aspirin", "Registered", null, workflowId),
            Projection("a3000000-0000-0000-0000-000000000005", "41000000-0000-0000-0000-000000000001", ReportingProjectionType.Screening, "CompletedScreeningSnapshot", "Algocura.Screening.Api", "Prototype TREM2 screening", "Alzheimer's disease", "TREM2", null, "Completed", 0.93m, workflowId),
            Projection("a3000000-0000-0000-0000-000000000006", "62000000-0000-0000-0000-000000000001", ReportingProjectionType.Assay, "AssaySnapshot", "Algocura.AssayManagement.Api", "Prototype TREM2 assay", "Alzheimer's disease", "TREM2", "Aspirin", "Published", 0.94m, workflowId),
            Projection("a3000000-0000-0000-0000-000000000007", "52000000-0000-0000-0000-000000000001", ReportingProjectionType.Prediction, "PredictionJobCompleted", "Algocura.AIDiscovery.Api", "TREM2 candidate prioritization", "Alzheimer's disease", "TREM2", "Aspirin", "Completed", 0.96m, workflowId),
            Projection("a3000000-0000-0000-0000-000000000008", "71000000-0000-0000-0000-000000000001", ReportingProjectionType.Workflow, "WorkflowSnapshot", "Algocura.Workflow.Api", "Prototype TREM2 discovery workflow", "Alzheimer's disease", "TREM2", null, "Completed", null, workflowId)
        };

        var ids = projections.Select(x => x.Id).ToArray();
        var existing = await db.ReportingProjections.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = projections
            .Where(x => !existing.Contains(x.Id))
            .ToArray();

        if (missing.Length > 0)
        {
            await db.ReportingProjections.AddRangeAsync(missing, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task SeedRunsAsync(
        ReportingDbContext db,
        CancellationToken cancellationToken)
    {
        var runs = new[]
        {
            Run(PlatformSummaryRunId, PlatformSummaryDefinitionId, "Seeded Platform Summary", "{}"),
            Run(DiscoveryFunnelRunId, DiscoveryFunnelDefinitionId, "Seeded TREM2 Discovery Funnel", "{\"diseaseName\":\"Alzheimer's disease\",\"geneSymbol\":\"TREM2\"}")
        };

        var ids = runs.Select(x => x.Id).ToArray();
        var existing = await db.ReportRuns.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = runs
            .Where(x => !existing.Contains(x.Id))
            .ToArray();

        if (missing.Length > 0)
        {
            await db.ReportRuns.AddRangeAsync(missing, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task SeedMetricsAsync(
        ReportingDbContext db,
        CancellationToken cancellationToken)
    {
        var metrics = new[]
        {
            Metric("a4000000-0000-0000-0000-000000000001", PlatformSummaryRunId, "TotalProjections", 8m, "records"),
            Metric("a4000000-0000-0000-0000-000000000002", PlatformSummaryRunId, "CompletedWorkflowCount", 1m, "workflows"),
            Metric("a4000000-0000-0000-0000-000000000003", DiscoveryFunnelRunId, "LiteratureRecords", 1m, "records"),
            Metric("a4000000-0000-0000-0000-000000000004", DiscoveryFunnelRunId, "SelectedTargets", 1m, "targets"),
            Metric("a4000000-0000-0000-0000-000000000005", DiscoveryFunnelRunId, "PrioritizedCandidates", 1m, "candidates")
        };

        var ids = metrics.Select(x => x.Id).ToArray();
        var existing = await db.ReportMetrics.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = metrics
            .Where(x => !existing.Contains(x.Id))
            .ToArray();

        if (missing.Length > 0)
        {
            await db.ReportMetrics.AddRangeAsync(missing, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static ReportDefinition Definition(Guid id, string name, string type, string description, string parameters) =>
        new()
        {
            Id = id,
            Name = name,
            ReportType = type,
            Description = description,
            DefaultParametersJson = parameters,
            Status = ReportDefinitionStatus.Active,
            OwnerUserId = SystemUsers.SystemUserId,
            ApprovedByUserId = SystemUsers.SystemUserId,
            ApprovedAtUtc = SeedTimestamp,
            CreatedByUserId = SystemUsers.SystemUserId,
            UpdatedByUserId = SystemUsers.SystemUserId,
            CreatedAtUtc = SeedTimestamp,
            UpdatedAtUtc = SeedTimestamp
        };

    private static ReportingProjection Projection(
        string id, string sourceId, ReportingProjectionType type, string eventType,
        string source, string title, string? disease, string? gene, string? compound,
        string? outcome, decimal? score, string workflowId) =>
        new()
        {
            Id = Guid.Parse(id),
            SourceEntityId = Guid.Parse(sourceId),
            ProjectionType = type,
            SourceEventType = eventType,
            SourceService = source,
            Title = title,
            Summary = $"Seeded reporting projection for {title}.",
            DiseaseName = disease,
            GeneSymbol = gene,
            CompoundName = compound,
            Outcome = outcome,
            Score = score,
            WorkflowId = workflowId,
            CorrelationId = "prototype-trem2-correlation",
            SourceUpdatedAtUtc = SeedTimestamp,
            SynchronizedAtUtc = SeedTimestamp,
            SourceCreatedByUserId = SystemUsers.SystemUserId,
            SourceUpdatedByUserId = SystemUsers.SystemUserId,
            CreatedByUserId = SystemUsers.SystemUserId,
            UpdatedByUserId = SystemUsers.SystemUserId,
            CreatedAtUtc = SeedTimestamp,
            UpdatedAtUtc = SeedTimestamp
        };

    private static ReportRun Run(Guid id, Guid definitionId, string name, string parameters) =>
        new()
        {
            Id = id,
            ReportDefinitionId = definitionId,
            Name = name,
            ParametersJson = parameters,
            Status = ReportRunStatus.Completed,
            ResultLocation = "database://report-metrics",
            StartedAtUtc = SeedTimestamp,
            StartedByUserId = SystemUsers.SystemUserId,
            CompletedByUserId = SystemUsers.SystemUserId,
            CompletedAtUtc = SeedTimestamp,
            OwnerUserId = SystemUsers.SystemUserId,
            CreatedByUserId = SystemUsers.SystemUserId,
            UpdatedByUserId = SystemUsers.SystemUserId,
            CreatedAtUtc = SeedTimestamp,
            UpdatedAtUtc = SeedTimestamp
        };

    private static ReportMetric Metric(string id, Guid runId, string name, decimal value, string unit) =>
        new()
        {
            Id = Guid.Parse(id),
            ReportRunId = runId,
            Name = name,
            Value = value,
            ValueType = MetricValueType.Count,
            Unit = unit,
            CreatedByUserId = SystemUsers.SystemUserId,
            UpdatedByUserId = SystemUsers.SystemUserId,
            CreatedAtUtc = SeedTimestamp,
            UpdatedAtUtc = SeedTimestamp
        };
}
