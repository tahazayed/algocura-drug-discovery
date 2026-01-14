using Algocura.Workflow.Api.Domain;
using Algocura.Workflow.Api.Messaging.Consumers;
using Algocura.Workflow.Api.Services;
using Algocura.Workflow.Api.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Workflow.Api.Tests.Messaging;

public sealed class PlatformWorkflowEventsConsumerTests
{
    [Fact]
    public async Task Handle_Event_CompletesStageAndStoresReceipt()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var workflow =
            CreateWorkflow();

        database.Context.Workflows.Add(
            workflow);

        await database.Context.SaveChangesAsync(
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        var eventId =
            Guid.NewGuid();

        await PlatformWorkflowEventsConsumer.HandleAsync(
            database.Context,
            new WorkflowStateMachine(),
            workflow.Id,
            eventId,
            "LiteratureIntelligenceSnapshot",
            "Algocura.LiteratureIntelligence.Api",
            DateTimeOffset.UtcNow,
            WorkflowStage.LiteratureIntelligence,
            Guid.NewGuid(),
            "TargetHypothesis",
            "Processed snapshot.",
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        var saved =
            await database.Context.Workflows
                .Include(x => x.Stages)
                .SingleAsync(
                    x => x.Id == workflow.Id,
                    timeout.Token);

        saved.CurrentStage.Should()
            .Be(WorkflowStage.TargetDiscovery);

        var receipts =
            await database.Context.WorkflowEventReceipts
                .CountAsync(timeout.Token);

        receipts.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ReplayedEvent_IsIdempotent()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var workflow =
            CreateWorkflow();

        database.Context.Workflows.Add(
            workflow);

        await database.Context.SaveChangesAsync(
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        var eventId =
            Guid.NewGuid();

        for (var index = 0;
             index < 2;
             index++)
        {
            await PlatformWorkflowEventsConsumer.HandleAsync(
                database.Context,
                new WorkflowStateMachine(),
                workflow.Id,
                eventId,
                "LiteratureIntelligenceSnapshot",
                "Literature",
                DateTimeOffset.UtcNow,
                WorkflowStage.LiteratureIntelligence,
                Guid.NewGuid(),
                "TargetHypothesis",
                "Processed snapshot.",
                timeout.Token);

            database.Context.ChangeTracker.Clear();
        }

        var count =
            await database.Context.WorkflowEventReceipts
                .CountAsync(timeout.Token);

        count.Should().Be(1);
    }

    private static DiscoveryWorkflow CreateWorkflow()
    {
        var now =
            DateTimeOffset.UtcNow;

        var workflow =
            new DiscoveryWorkflow
            {
                Id = Guid.NewGuid(),
                Name = "Workflow",
                DiseaseName = "Disease",
                Status =
                    DiscoveryWorkflowStatus.InProgress,
                CurrentStage =
                    WorkflowStage.LiteratureIntelligence,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                StartedAtUtc = now
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
                        stage ==
                        WorkflowStage.LiteratureIntelligence
                            ? WorkflowStageStatus.InProgress
                            : WorkflowStageStatus.Pending,
                    StartedAtUtc =
                        stage ==
                        WorkflowStage.LiteratureIntelligence
                            ? now
                            : null,
                    UpdatedAtUtc = now
                });
        }

        return workflow;
    }
}
