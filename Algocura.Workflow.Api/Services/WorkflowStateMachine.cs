using Algocura.Workflow.Api.Domain;

namespace Algocura.Workflow.Api.Services;

public interface IWorkflowStateMachine
{
    void Start(DiscoveryWorkflow workflow, DateTimeOffset now);

    void CompleteStage(
        DiscoveryWorkflow workflow,
        WorkflowStage stage,
        Guid? externalEntityId,
        string? externalEntityType,
        string? summary,
        DateTimeOffset now);

    void FailStage(
        DiscoveryWorkflow workflow,
        WorkflowStage stage,
        string failureReason,
        DateTimeOffset now);
}

public sealed class WorkflowStateMachine
    : IWorkflowStateMachine
{
    public void Start(
        DiscoveryWorkflow workflow,
        DateTimeOffset now)
    {
        if (workflow.Status !=
            DiscoveryWorkflowStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only draft workflows can be started.");
        }

        workflow.Status =
            DiscoveryWorkflowStatus.InProgress;

        workflow.CurrentStage =
            WorkflowStage.LiteratureIntelligence;

        workflow.StartedAtUtc = now;
        workflow.UpdatedAtUtc = now;

        var firstStage =
            GetStage(
                workflow,
                WorkflowStage.LiteratureIntelligence);

        firstStage.Status =
            WorkflowStageStatus.InProgress;

        firstStage.StartedAtUtc = now;
        firstStage.UpdatedAtUtc = now;
    }

    public void CompleteStage(
        DiscoveryWorkflow workflow,
        WorkflowStage stage,
        Guid? externalEntityId,
        string? externalEntityType,
        string? summary,
        DateTimeOffset now)
    {
        if (workflow.Status is
            DiscoveryWorkflowStatus.Cancelled or
            DiscoveryWorkflowStatus.Completed)
        {
            throw new InvalidOperationException(
                "A finalized workflow cannot be advanced.");
        }

        var stageExecution =
            GetStage(workflow, stage);

        if (stageExecution.Status ==
            WorkflowStageStatus.Completed)
        {
            return;
        }

        stageExecution.Status =
            WorkflowStageStatus.Completed;

        stageExecution.ExternalEntityId =
            externalEntityId;

        stageExecution.ExternalEntityType =
            externalEntityType;

        stageExecution.Summary =
            summary;

        stageExecution.CompletedAtUtc = now;
        stageExecution.UpdatedAtUtc = now;

        workflow.Status =
            DiscoveryWorkflowStatus.InProgress;

        workflow.UpdatedAtUtc = now;

        var nextStage =
            GetNextStage(stage);

        if (nextStage is null)
        {
            workflow.Status =
                DiscoveryWorkflowStatus.Completed;

            workflow.CurrentStage = stage;
            workflow.CompletedAtUtc = now;

            return;
        }

        workflow.CurrentStage =
            nextStage.Value;

        var nextExecution =
            GetStage(workflow, nextStage.Value);

        if (nextExecution.Status ==
            WorkflowStageStatus.Pending)
        {
            nextExecution.Status =
                WorkflowStageStatus.InProgress;

            nextExecution.StartedAtUtc = now;
            nextExecution.UpdatedAtUtc = now;
        }
    }

    public void FailStage(
        DiscoveryWorkflow workflow,
        WorkflowStage stage,
        string failureReason,
        DateTimeOffset now)
    {
        var execution =
            GetStage(workflow, stage);

        execution.Status =
            WorkflowStageStatus.Failed;

        execution.FailureReason =
            failureReason.Trim();

        execution.CompletedAtUtc = now;
        execution.UpdatedAtUtc = now;

        workflow.Status =
            DiscoveryWorkflowStatus.Failed;

        workflow.CurrentStage = stage;
        workflow.UpdatedAtUtc = now;
    }

    private static WorkflowStageExecution GetStage(
        DiscoveryWorkflow workflow,
        WorkflowStage stage)
    {
        return workflow.Stages.Single(
            x => x.Stage == stage);
    }

    private static WorkflowStage? GetNextStage(
        WorkflowStage stage)
    {
        return stage switch
        {
            WorkflowStage.LiteratureIntelligence =>
                WorkflowStage.TargetDiscovery,

            WorkflowStage.TargetDiscovery =>
                WorkflowStage.ChemicalDesign,

            WorkflowStage.ChemicalDesign =>
                WorkflowStage.CompoundCatalog,

            WorkflowStage.CompoundCatalog =>
                WorkflowStage.Screening,

            WorkflowStage.Screening =>
                WorkflowStage.AssayManagement,

            WorkflowStage.AssayManagement =>
                WorkflowStage.AiDiscovery,

            WorkflowStage.AiDiscovery =>
                null,

            _ => throw new ArgumentOutOfRangeException(
                nameof(stage),
                stage,
                null)
        };
    }
}
