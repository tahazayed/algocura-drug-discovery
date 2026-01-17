using Algocura.Reporting.Api.Data;
using Algocura.Reporting.Api.Domain;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Algocura.Reporting.Api.Services;

public interface IReportExecutionService
{
    Task<ReportRun> ExecuteAsync(
        ReportDefinition definition,
        string runName,
        string? parametersJson,
        CancellationToken cancellationToken = default);
}

public sealed class ReportExecutionService(
    ReportingDbContext db)
    : IReportExecutionService
{
    public async Task<ReportRun> ExecuteAsync(
        ReportDefinition definition,
        string runName,
        string? parametersJson,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var run = new ReportRun
        {
            Id = Guid.NewGuid(),
            ReportDefinitionId = definition.Id,
            Name = runName,
            ParametersJson = parametersJson,
            Status = ReportRunStatus.Running,
            StartedAtUtc = now,
            StartedByUserId = SystemUsers.SystemUserId,
            OwnerUserId = SystemUsers.SystemUserId,
            CreatedByUserId = SystemUsers.SystemUserId,
            UpdatedByUserId = SystemUsers.SystemUserId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await db.ReportRuns.AddAsync(run, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            var metrics = definition.ReportType switch
            {
                "PlatformSummary" =>
                    await BuildPlatformSummaryAsync(
                        run.Id,
                        cancellationToken),

                "DiscoveryFunnel" =>
                    await BuildDiscoveryFunnelAsync(
                        run.Id,
                        parametersJson,
                        cancellationToken),

                _ =>
                    throw new InvalidOperationException(
                        $"Unsupported report type '{definition.ReportType}'.")
            };

            await db.ReportMetrics.AddRangeAsync(
                metrics,
                cancellationToken);

            run.Status = ReportRunStatus.Completed;
            run.ResultLocation = "database://report-metrics";
            run.CompletedByUserId = SystemUsers.SystemUserId;
            run.CompletedAtUtc = DateTimeOffset.UtcNow;
            run.UpdatedByUserId = SystemUsers.SystemUserId;
            run.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync(cancellationToken);

            run.Metrics = metrics.ToList();
            return run;
        }
        catch (Exception exception)
        {
            run.Status = ReportRunStatus.Failed;
            run.FailureReason = exception.Message;
            run.CompletedByUserId = SystemUsers.SystemUserId;
            run.CompletedAtUtc = DateTimeOffset.UtcNow;
            run.UpdatedByUserId = SystemUsers.SystemUserId;
            run.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    private async Task<IReadOnlyCollection<ReportMetric>>
        BuildPlatformSummaryAsync(
            Guid runId,
            CancellationToken cancellationToken)
    {
        var total = await db.ReportingProjections
            .CountAsync(cancellationToken);

        var completedWorkflows =
            await db.ReportingProjections.CountAsync(
                x =>
                    x.ProjectionType ==
                        ReportingProjectionType.Workflow &&
                    x.Outcome == "Completed",
                cancellationToken);

        return
        [
            Metric(runId, "TotalProjections", total, "records"),
            Metric(runId, "CompletedWorkflowCount", completedWorkflows, "workflows")
        ];
    }

    private async Task<IReadOnlyCollection<ReportMetric>>
        BuildDiscoveryFunnelAsync(
            Guid runId,
            string? parametersJson,
            CancellationToken cancellationToken)
    {
        string? diseaseName = null;
        string? geneSymbol = null;

        if (!string.IsNullOrWhiteSpace(parametersJson))
        {
            using var document = JsonDocument.Parse(parametersJson);

            if (document.RootElement.TryGetProperty(
                    "diseaseName",
                    out var disease))
            {
                diseaseName = disease.GetString();
            }

            if (document.RootElement.TryGetProperty(
                    "geneSymbol",
                    out var gene))
            {
                geneSymbol = gene.GetString();
            }
        }

        var query = db.ReportingProjections.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(diseaseName))
        {
            query = query.Where(x => x.DiseaseName == diseaseName);
        }

        if (!string.IsNullOrWhiteSpace(geneSymbol))
        {
            query = query.Where(x => x.GeneSymbol == geneSymbol);
        }

        var literature = await query.CountAsync(
            x => x.ProjectionType == ReportingProjectionType.Literature,
            cancellationToken);

        var targets = await query.CountAsync(
            x =>
                x.ProjectionType == ReportingProjectionType.Target &&
                x.Outcome == "Selected",
            cancellationToken);

        var predictions = await query.CountAsync(
            x => x.ProjectionType == ReportingProjectionType.Prediction,
            cancellationToken);

        return
        [
            Metric(runId, "LiteratureRecords", literature, "records"),
            Metric(runId, "SelectedTargets", targets, "targets"),
            Metric(runId, "PrioritizedCandidates", predictions, "candidates")
        ];
    }

    private static ReportMetric Metric(
        Guid runId,
        string name,
        decimal value,
        string unit)
    {
        var now = DateTimeOffset.UtcNow;

        return new ReportMetric
        {
            Id = Guid.NewGuid(),
            ReportRunId = runId,
            Name = name,
            Value = value,
            ValueType = MetricValueType.Count,
            Unit = unit,
            CreatedByUserId = SystemUsers.SystemUserId,
            UpdatedByUserId = SystemUsers.SystemUserId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }
}
