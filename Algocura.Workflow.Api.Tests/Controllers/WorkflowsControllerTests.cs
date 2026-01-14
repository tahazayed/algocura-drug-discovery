using Algocura.Workflow.Api.Contracts;
using Algocura.Workflow.Api.Controllers;
using Algocura.Workflow.Api.Domain;
using Algocura.Workflow.Api.Events;
using Algocura.Workflow.Api.Messaging.Publishers;
using Algocura.Workflow.Api.Services;
using Algocura.Workflow.Api.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Algocura.Workflow.Api.Tests.Controllers;

public sealed class WorkflowsControllerTests
{
    [Fact]
    public async Task Create_ValidRequest_CreatesSevenStages()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var publisher =
            Substitute.For<IEventPublisher>();

        var controller =
            new WorkflowsController(
                database.Context,
                new WorkflowStateMachine(),
                publisher);

        var action =
            await controller.Create(
                new CreateWorkflowRequest(
                    "Parkinson workflow",
                    "Parkinson disease",
                    "LRRK2",
                    "Find and prioritize candidates."),
                timeout.Token);

        var created =
            action.Result.Should()
                .BeOfType<CreatedAtActionResult>()
                .Subject;

        var response =
            created.Value.Should()
                .BeOfType<DiscoveryWorkflowResponse>()
                .Subject;

        response.Stages.Should()
            .HaveCount(7);

        response.Status.Should()
            .Be(DiscoveryWorkflowStatus.Draft);

        await publisher.Received(1).PublishAsync(
            Arg.Any<WorkflowCreated>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}
