using Algocura.Workflow.Api.Data;
using Algocura.Workflow.Api.Events;
using Algocura.Workflow.Api.Messaging.Publishers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Workflow.Api.Controllers;

[ApiController]
[Route("api/v1/workflow/synchronization")]
public sealed class WorkflowSynchronizationController(
    WorkflowDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<WorkflowSynchronizationController> logger)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<WorkflowSynchronizationResponse>>
        SynchronizeWorkflows(
            CancellationToken cancellationToken = default)
    {
        var startedAtUtc =
            DateTimeOffset.UtcNow;

        var workflows =
            await dbContext.Workflows
                .AsNoTracking()
                .Include(x => x.Stages)
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

        var published = 0;
        var failures =
            new List<WorkflowSynchronizationFailure>();

        foreach (var workflow in workflows)
        {
            try
            {
                await eventPublisher.PublishAsync(
                    CreateSnapshot(
                        workflow,
                        DateTimeOffset.UtcNow),
                    $"workflow-sync-{workflow.Id}",
                    cancellationToken);

                published++;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to synchronize workflow {WorkflowId}.",
                    workflow.Id);

                failures.Add(
                    new WorkflowSynchronizationFailure(
                        workflow.Id,
                        exception.Message));
            }
        }

        return Ok(
            new WorkflowSynchronizationResponse(
                workflows.Count,
                published,
                failures.Count,
                startedAtUtc,
                DateTimeOffset.UtcNow,
                failures));
    }

    [HttpPost("{id:guid}")]
    public async Task<ActionResult<WorkflowSynchronizationItemResponse>>
        SynchronizeById(
            Guid id,
            CancellationToken cancellationToken = default)
    {
        var workflow =
            await dbContext.Workflows
                .AsNoTracking()
                .Include(x => x.Stages)
                .SingleOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

        if (workflow is null)
        {
            return NotFound();
        }

        var publishedAtUtc =
            DateTimeOffset.UtcNow;

        await eventPublisher.PublishAsync(
            CreateSnapshot(
                workflow,
                publishedAtUtc),
            $"workflow-sync-{workflow.Id}",
            cancellationToken);

        return Ok(
            new WorkflowSynchronizationItemResponse(
                workflow.Id,
                workflow.Name,
                publishedAtUtc));
    }

    private static WorkflowSnapshot CreateSnapshot(
        Domain.DiscoveryWorkflow workflow,
        DateTimeOffset occurredAtUtc)
    {
        return new WorkflowSnapshot(
            Guid.NewGuid(),
            occurredAtUtc,
            workflow.Id,
            workflow.Name,
            workflow.DiseaseName,
            workflow.GeneSymbol,
            workflow.Status.ToString(),
            workflow.CurrentStage.ToString(),
            workflow.Stages
                .OrderBy(x => x.Stage)
                .Select(
                    stage =>
                        new WorkflowStageSnapshot(
                            stage.Stage.ToString(),
                            stage.Status.ToString(),
                            stage.ExternalEntityId,
                            stage.ExternalEntityType,
                            stage.Summary))
                .ToArray());
    }
}

public sealed record WorkflowSynchronizationResponse(
    int TotalWorkflows,
    int PublishedEvents,
    int FailedEvents,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    IReadOnlyCollection<WorkflowSynchronizationFailure> Failures);

public sealed record WorkflowSynchronizationFailure(
    Guid WorkflowId,
    string Error);

public sealed record WorkflowSynchronizationItemResponse(
    Guid WorkflowId,
    string WorkflowName,
    DateTimeOffset PublishedAtUtc);
