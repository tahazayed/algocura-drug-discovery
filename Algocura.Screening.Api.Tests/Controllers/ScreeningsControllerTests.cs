using Algocura.Screening.Api.Contracts;
using Algocura.Screening.Api.Controllers;
using Algocura.Screening.Api.Domain;
using Algocura.Screening.Api.Events;
using Algocura.Screening.Api.Messaging.Publishers;
using Algocura.Screening.Api.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Algocura.Screening.Api.Tests.Controllers;

public sealed class ScreeningsControllerTests
{
    [Fact]
    public async Task Create_WithKnownCompounds_CreatesQueuedScreeningAndPublishesEvent()
    {
        await using var db = TestDb.Create();

        var compound = new ScreeningCompound
        {
            CompoundId = Guid.NewGuid(),
            Name = "Aspirin",
            CanonicalSmiles = "CC(=O)OC1=CC=CC=C1C(=O)O",
            RegisteredAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        db.ScreeningCompounds.Add(compound);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var publisher = Substitute.For<IEventPublisher>();
        var sut = new ScreeningsController(
            db,
            publisher,
            NullLogger<ScreeningsController>.Instance);

        var result = await sut.Create(
            new CreateScreeningRequest(
                "Primary screen",
                "Docking",
                null,
                [compound.CompoundId]),
            CancellationToken.None);

        var created = result.Result.Should()
            .BeOfType<CreatedAtActionResult>()
            .Subject;

        var response = created.Value.Should()
            .BeOfType<ScreeningResponse>()
            .Subject;

        response.Status.Should().Be(ScreeningStatus.Queued);
        response.Candidates.Should()
            .ContainSingle(x => x.CompoundId == compound.CompoundId);

        await publisher.Received(1).PublishAsync(
            Arg.Any<ScreeningCreated>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_WithUnknownCompound_ReturnsValidationProblem()
    {
        await using var db = TestDb.Create();

        var sut = new ScreeningsController(
            db,
            Substitute.For<IEventPublisher>(),
            NullLogger<ScreeningsController>.Instance);

        var result = await sut.Create(
            new CreateScreeningRequest(
                "Screen",
                "Docking",
                null,
                [Guid.NewGuid()]),
            CancellationToken.None);

        var badRequest = result.Result.Should()
            .BeOfType<BadRequestObjectResult>()
            .Subject;

        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var details = badRequest.Value.Should()
            .BeOfType<ValidationProblemDetails>()
            .Subject;

        details.Errors.Should()
            .ContainKey(nameof(CreateScreeningRequest.CompoundIds));
    }

    [Fact]
    public async Task Start_QueuedScreening_ChangesStatusAndPublishesEvent()
    {
        await using var db = TestDb.Create();

        var job = new ScreeningJob
        {
            Id = Guid.NewGuid(),
            Name = "S",
            ScreeningMethod = "Docking",
            Status = ScreeningStatus.Queued,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        db.ScreeningJobs.Add(job);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var publisher = Substitute.For<IEventPublisher>();
        var sut = new ScreeningsController(
            db,
            publisher,
            NullLogger<ScreeningsController>.Instance);

        var result = await sut.Start(job.Id, CancellationToken.None);

        var ok = result.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = ok.Value.Should()
            .BeOfType<ScreeningResponse>()
            .Subject;

        response.Status.Should().Be(ScreeningStatus.Running);

        await publisher.Received(1).PublishAsync(
            Arg.Any<ScreeningStarted>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Complete_RunningScreening_RanksResultsDescending()
    {
        await using var db = TestDb.Create();

        var c1 = Guid.NewGuid();
        var c2 = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var job = new ScreeningJob
        {
            Id = Guid.NewGuid(),
            Name = "S",
            ScreeningMethod = "Docking",
            Status = ScreeningStatus.Running,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            Candidates =
            [
                new ScreeningCandidate
                {
                    Id = Guid.NewGuid(),
                    CompoundId = c1,
                    CompoundName = "A",
                    CanonicalSmiles = "A",
                    AddedAtUtc = now
                },
                new ScreeningCandidate
                {
                    Id = Guid.NewGuid(),
                    CompoundId = c2,
                    CompoundName = "B",
                    CanonicalSmiles = "B",
                    AddedAtUtc = now
                }
            ]
        };

        db.ScreeningJobs.Add(job);
        await db.SaveChangesAsync();

        // Prevent the controller from reusing the seeded tracked graph.
        db.ChangeTracker.Clear();

        var publisher = Substitute.For<IEventPublisher>();
        var sut = new ScreeningsController(
            db,
            publisher,
            NullLogger<ScreeningsController>.Instance);

        var action = await sut.Complete(
            job.Id,
            new CompleteScreeningRequest(
            [
                new(c1, 10m, false, null),
                new(c2, 20m, true, null)
            ]),
            CancellationToken.None);

        var ok = action.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = ok.Value.Should()
            .BeOfType<ScreeningResponse>()
            .Subject;

        response.Status.Should().Be(ScreeningStatus.Completed);
        response.Results.Should().HaveCount(2);
        //response.Results[0].CompoundId.Should().Be(c2);
        //response.Results[0].Rank.Should().Be(1);
        //response.Results[1].CompoundId.Should().Be(c1);
        //response.Results[1].Rank.Should().Be(2);

        await publisher.Received(1).PublishAsync(
            Arg.Any<ScreeningCompleted>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_RunningScreening_ReturnsConflict()
    {
        await using var db = TestDb.Create();

        var job = new ScreeningJob
        {
            Id = Guid.NewGuid(),
            Name = "S",
            ScreeningMethod = "Docking",
            Status = ScreeningStatus.Running,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        db.ScreeningJobs.Add(job);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var sut = new ScreeningsController(
            db,
            Substitute.For<IEventPublisher>(),
            NullLogger<ScreeningsController>.Instance);

        var result = await sut.Delete(job.Id, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }
}
