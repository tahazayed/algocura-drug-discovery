#pragma warning disable xUnit1051

using Algocura.Platform.Messaging.Publishing;
using Algocura.Screening.Api.Controllers;
using Algocura.Screening.Api.Domain;
using Algocura.Screening.Api.Events;
using Algocura.Screening.Api.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Algocura.Screening.Api.Tests.Controllers;

public sealed class ScreeningSynchronizationControllerTests
{
    [Fact]
    public async Task SynchronizeAllCompleted_PublishesSnapshotForEachCompletedScreening()
    {
        await using var dbContext = TestDb.Create();

        var firstScreening = CreateCompletedScreening(
            "Primary Screening",
            [
                CreateResult(
                    score: 91.25m,
                    rank: 1,
                    selected: true),

                CreateResult(
                    score: 78.50m,
                    rank: 2,
                    selected: false)
            ]);

        var secondScreening = CreateCompletedScreening(
            "Secondary Screening",
            [
                CreateResult(
                    score: 88.10m,
                    rank: 1,
                    selected: true)
            ]);

        var runningScreening = new ScreeningJob
        {
            Id = Guid.NewGuid(),
            Name = "Running Screening",
            ScreeningMethod = "Molecular Docking",
            Status = ScreeningStatus.Running,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.ScreeningJobs.AddRange(
            firstScreening,
            secondScreening,
            runningScreening);

        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        var eventPublisher =
            Substitute.For<IEventPublisher>();

        var controller =
            new ScreeningSynchronizationController(
                dbContext,
                eventPublisher,
                NullLogger<
                    ScreeningSynchronizationController>.Instance);

        var actionResult =
            await controller.SynchronizeAllCompleted(
                CancellationToken.None);

        var okResult = actionResult.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = okResult.Value.Should()
            .BeOfType<ScreeningSynchronizationResponse>()
            .Subject;

        response.TotalCompletedScreenings.Should().Be(2);
        response.PublishedEvents.Should().Be(2);
        response.FailedEvents.Should().Be(0);
        response.Failures.Should().BeEmpty();

        await eventPublisher.Received(2)
            .PublishAsync(
                Arg.Any<CompletedScreeningSnapshot>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAllCompleted_WhenNoCompletedScreenings_ReturnsEmptyResult()
    {
        await using var dbContext = TestDb.Create();

        dbContext.ScreeningJobs.Add(
            new ScreeningJob
            {
                Id = Guid.NewGuid(),
                Name = "Queued Screening",
                ScreeningMethod = "Molecular Docking",
                Status = ScreeningStatus.Queued,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            });

        await dbContext.SaveChangesAsync();

        var eventPublisher =
            Substitute.For<IEventPublisher>();

        var controller =
            new ScreeningSynchronizationController(
                dbContext,
                eventPublisher,
                NullLogger<
                    ScreeningSynchronizationController>.Instance);

        var actionResult =
            await controller.SynchronizeAllCompleted(
                CancellationToken.None);

        var okResult = actionResult.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = okResult.Value.Should()
            .BeOfType<ScreeningSynchronizationResponse>()
            .Subject;

        response.TotalCompletedScreenings.Should().Be(0);
        response.PublishedEvents.Should().Be(0);
        response.FailedEvents.Should().Be(0);

        await eventPublisher.DidNotReceive()
            .PublishAsync(
                Arg.Any<CompletedScreeningSnapshot>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeById_CompletedScreening_PublishesSnapshot()
    {
        await using var dbContext = TestDb.Create();

        var screening = CreateCompletedScreening(
            "Completed Screening",
            [
                CreateResult(
                    score: 94.75m,
                    rank: 1,
                    selected: true)
            ]);

        dbContext.ScreeningJobs.Add(screening);

        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        var eventPublisher =
            Substitute.For<IEventPublisher>();

        var controller =
            new ScreeningSynchronizationController(
                dbContext,
                eventPublisher,
                NullLogger<
                    ScreeningSynchronizationController>.Instance);

        var actionResult =
            await controller.SynchronizeById(
                screening.Id,
                CancellationToken.None);

        var okResult = actionResult.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = okResult.Value.Should()
            .BeOfType<
                ScreeningSynchronizationItemResponse>()
            .Subject;

        response.ScreeningId.Should().Be(screening.Id);
        response.ScreeningName.Should().Be(screening.Name);
        response.ResultCount.Should().Be(1);

        await eventPublisher.Received(1)
            .PublishAsync(
                Arg.Is<CompletedScreeningSnapshot>(
                    integrationEvent =>
                        integrationEvent != null &&
                        integrationEvent.ScreeningId ==
                            screening.Id &&
                        integrationEvent.Name ==
                            screening.Name &&
                        integrationEvent.Results.Count == 1),
                $"screening-sync-{screening.Id}",
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeById_UnknownScreening_ReturnsNotFound()
    {
        await using var dbContext = TestDb.Create();

        var controller =
            new ScreeningSynchronizationController(
                dbContext,
                Substitute.For<IEventPublisher>(),
                NullLogger<
                    ScreeningSynchronizationController>.Instance);

        var actionResult =
            await controller.SynchronizeById(
                Guid.NewGuid(),
                CancellationToken.None);

        actionResult.Result.Should()
            .BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task SynchronizeById_NonCompletedScreening_ReturnsConflict()
    {
        await using var dbContext = TestDb.Create();

        var screening = new ScreeningJob
        {
            Id = Guid.NewGuid(),
            Name = "Running Screening",
            ScreeningMethod = "Molecular Docking",
            Status = ScreeningStatus.Running,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.ScreeningJobs.Add(screening);

        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        var eventPublisher =
            Substitute.For<IEventPublisher>();

        var controller =
            new ScreeningSynchronizationController(
                dbContext,
                eventPublisher,
                NullLogger<
                    ScreeningSynchronizationController>.Instance);

        var actionResult =
            await controller.SynchronizeById(
                screening.Id,
                CancellationToken.None);

        var conflictResult = actionResult.Result.Should()
            .BeOfType<ConflictObjectResult>()
            .Subject;

        var problemDetails = conflictResult.Value.Should()
            .BeOfType<ProblemDetails>()
            .Subject;

        problemDetails.Status.Should()
            .Be(StatusCodes.Status409Conflict);

        problemDetails.Title.Should()
            .Be("Screening is not completed");

        await eventPublisher.DidNotReceive()
            .PublishAsync(
                Arg.Any<CompletedScreeningSnapshot>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeAllCompleted_WhenPublishingFails_ReportsFailure()
    {
        await using var dbContext = TestDb.Create();

        var screening = CreateCompletedScreening(
            "Failed Synchronization",
            [
                CreateResult(
                    score: 80m,
                    rank: 1,
                    selected: true)
            ]);

        dbContext.ScreeningJobs.Add(screening);

        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        var eventPublisher =
            Substitute.For<IEventPublisher>();

        eventPublisher
            .PublishAsync(
                Arg.Any<CompletedScreeningSnapshot>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromException(
                    new InvalidOperationException(
                        "Service Bus is unavailable.")));

        var controller =
            new ScreeningSynchronizationController(
                dbContext,
                eventPublisher,
                NullLogger<
                    ScreeningSynchronizationController>.Instance);

        var actionResult =
            await controller.SynchronizeAllCompleted(
                CancellationToken.None);

        var okResult = actionResult.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = okResult.Value.Should()
            .BeOfType<ScreeningSynchronizationResponse>()
            .Subject;

        response.TotalCompletedScreenings.Should().Be(1);
        response.PublishedEvents.Should().Be(0);
        response.FailedEvents.Should().Be(1);

        response.Failures.Should()
            .ContainSingle(failure =>
                failure.ScreeningId == screening.Id &&
                failure.ScreeningName == screening.Name);
    }

    private static ScreeningJob CreateCompletedScreening(
        string name,
        IReadOnlyCollection<ScreeningResult> results)
    {
        var now = DateTimeOffset.UtcNow;

        var screening = new ScreeningJob
        {
            Id = Guid.NewGuid(),
            Name = name,
            ScreeningMethod = "Molecular Docking",
            Status = ScreeningStatus.Completed,
            CreatedAtUtc = now.AddMinutes(-10),
            UpdatedAtUtc = now,
            CompletedAtUtc = now
        };

        foreach (var result in results)
        {
            result.ScreeningJobId = screening.Id;
            screening.Results.Add(result);
        }

        return screening;
    }

    private static ScreeningResult CreateResult(
        decimal score,
        int rank,
        bool selected)
    {
        return new ScreeningResult
        {
            Id = Guid.NewGuid(),
            CompoundId = Guid.NewGuid(),
            Score = score,
            Rank = rank,
            Selected = selected
        };
    }
}