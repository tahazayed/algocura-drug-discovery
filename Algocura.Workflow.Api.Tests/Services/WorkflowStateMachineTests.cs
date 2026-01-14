using Algocura.Workflow.Api.Domain;
using Algocura.Workflow.Api.Services;
using FluentAssertions;

namespace Algocura.Workflow.Api.Tests.Services;

public sealed class WorkflowStateMachineTests
{
    [Fact]
    public void Start_DraftWorkflow_ActivatesLiteratureStage()
    {
        var workflow =
            CreateWorkflow();

        var sut =
            new WorkflowStateMachine();

        var now =
            DateTimeOffset.UtcNow;

        sut.Start(
            workflow,
            now);

        workflow.Status.Should()
            .Be(DiscoveryWorkflowStatus.InProgress);

        workflow.CurrentStage.Should()
            .Be(WorkflowStage.LiteratureIntelligence);

        workflow.Stages.Single(
                x => x.Stage ==
                     WorkflowStage.LiteratureIntelligence)
            .Status.Should()
            .Be(WorkflowStageStatus.InProgress);
    }

    [Fact]
    public void CompleteStage_Literature_ActivatesTargetDiscovery()
    {
        var workflow =
            CreateWorkflow();

        var sut =
            new WorkflowStateMachine();

        var now =
            DateTimeOffset.UtcNow;

        sut.Start(workflow, now);

        sut.CompleteStage(
            workflow,
            WorkflowStage.LiteratureIntelligence,
            Guid.NewGuid(),
            "TargetHypothesis",
            "Literature complete.",
            now.AddMinutes(1));

        workflow.CurrentStage.Should()
            .Be(WorkflowStage.TargetDiscovery);

        workflow.Stages.Single(
                x => x.Stage ==
                     WorkflowStage.TargetDiscovery)
            .Status.Should()
            .Be(WorkflowStageStatus.InProgress);
    }

    private static DiscoveryWorkflow CreateWorkflow()
    {
        var workflow =
            new DiscoveryWorkflow
            {
                Id = Guid.NewGuid(),
                Name = "Workflow",
                DiseaseName = "Disease",
                Status =
                    DiscoveryWorkflowStatus.Draft,
                CurrentStage =
                    WorkflowStage.LiteratureIntelligence,
                CreatedAtUtc =
                    DateTimeOffset.UtcNow,
                UpdatedAtUtc =
                    DateTimeOffset.UtcNow
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
                    UpdatedAtUtc =
                        DateTimeOffset.UtcNow
                });
        }

        return workflow;
    }
}
