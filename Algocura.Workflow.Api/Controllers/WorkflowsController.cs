using Algocura.Platform.Messaging.Publishing;
using Algocura.Workflow.Api.Contracts;
using Algocura.Workflow.Api.Data;
using Algocura.Workflow.Api.Domain;
using Algocura.Workflow.Api.Events;
using Algocura.Workflow.Api.Mapping;
using Algocura.Workflow.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Workflow.Api.Controllers;

[ApiController]
[Route("api/v1/workflows")]
public sealed class WorkflowsController(
    WorkflowDbContext dbContext,
    IWorkflowStateMachine stateMachine,
    IEventPublisher eventPublisher)
    : ControllerBase
{
    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyCollection<DiscoveryWorkflowResponse>>> GetAll(
            DiscoveryWorkflowStatus? status,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
    {
        var query =
            dbContext.Workflows
                .AsNoTracking()
                .Include(x => x.Stages)
                .AsQueryable();

        if (status.HasValue)
        {
            query =
                query.Where(
                    x => x.Status == status.Value);
        }

        var workflows =
            await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .Skip(Math.Max(skip, 0))
                .Take(Math.Clamp(take, 1, 200))
                .ToListAsync(cancellationToken);

        return Ok(
            workflows
                .Select(x => x.ToResponse())
                .ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DiscoveryWorkflowResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var workflow =
            await LoadAsync(
                id,
                cancellationToken);

        return workflow is null
            ? NotFound()
            : Ok(workflow.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<DiscoveryWorkflowResponse>> Create(
        CreateWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var now =
            DateTimeOffset.UtcNow;

        var workflow =
            new DiscoveryWorkflow
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                DiseaseName =
                    request.DiseaseName.Trim(),
                GeneSymbol =
                    request.GeneSymbol?
                        .Trim()
                        .ToUpperInvariant(),
                Objective =
                    request.Objective?.Trim(),
                Status =
                    DiscoveryWorkflowStatus.Draft,
                CurrentStage =
                    WorkflowStage.LiteratureIntelligence,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

        foreach (var stage in
                 Enum.GetValues<WorkflowStage>())
        {
            workflow.Stages.Add(
                new WorkflowStageExecution
                {
                    Id = Guid.NewGuid(),
                    WorkflowId = workflow.Id,
                    Stage = stage,
                    Status =
                        WorkflowStageStatus.Pending,
                    UpdatedAtUtc = now
                });
        }

        dbContext.Workflows.Add(workflow);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        await eventPublisher.PublishAsync(
            new WorkflowCreated(
                Guid.NewGuid(),
                now,
                workflow.Id,
                workflow.Name,
                workflow.DiseaseName,
                workflow.GeneSymbol),
            workflow.Id.ToString(),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = workflow.Id },
            workflow.ToResponse());
    }

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<DiscoveryWorkflowResponse>> Start(
        Guid id,
        CancellationToken cancellationToken)
    {
        var workflow =
            await LoadAsync(
                id,
                cancellationToken);

        if (workflow is null)
        {
            return NotFound();
        }

        stateMachine.Start(
            workflow,
            DateTimeOffset.UtcNow);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return Ok(workflow.ToResponse());
    }

    [HttpPost("{id:guid}/stages/{stage}/fail")]
    public async Task<ActionResult<DiscoveryWorkflowResponse>> FailStage(
        Guid id,
        WorkflowStage stage,
        FailWorkflowStageRequest request,
        CancellationToken cancellationToken)
    {
        var workflow =
            await LoadAsync(
                id,
                cancellationToken);

        if (workflow is null)
        {
            return NotFound();
        }

        stateMachine.FailStage(
            workflow,
            stage,
            request.FailureReason,
            DateTimeOffset.UtcNow);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return Ok(workflow.ToResponse());
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<DiscoveryWorkflowResponse>> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        var workflow =
            await LoadAsync(
                id,
                cancellationToken);

        if (workflow is null)
        {
            return NotFound();
        }

        if (workflow.Status ==
            DiscoveryWorkflowStatus.Completed)
        {
            return Conflict(
                new ProblemDetails
                {
                    Title =
                        "Workflow is completed",
                    Detail =
                        "A completed workflow cannot be cancelled.",
                    Status =
                        StatusCodes.Status409Conflict
                });
        }

        workflow.Status =
            DiscoveryWorkflowStatus.Cancelled;

        workflow.UpdatedAtUtc =
            DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return Ok(workflow.ToResponse());
    }

    private Task<DiscoveryWorkflow?> LoadAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return dbContext.Workflows
            .Include(x => x.Stages)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }
}
