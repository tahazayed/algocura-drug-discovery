using Algocura.AssayManagement.Api.Contracts;
using Algocura.AssayManagement.Api.Controllers;
using Algocura.AssayManagement.Api.Domain;
using Algocura.AssayManagement.Api.Events;
using Algocura.AssayManagement.Api.Services;
using Algocura.AssayManagement.Api.Tests.Support;
using Algocura.Platform.Messaging.Publishing;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Algocura.AssayManagement.Api.Tests.Controllers;

public sealed class AssaysControllerTests
{
    [Fact]
    public async Task Create_KnownScreening_CreatesDraftAssay()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var screening =
            CreateScreening();

        database.Context.Screenings.Add(
            screening);

        await database.Context.SaveChangesAsync(
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        var publisher =
            Substitute.For<IEventPublisher>();

        var controller =
            CreateController(
                database.Context,
                publisher);

        var action = await controller.Create(
            new CreateAssayRequest(
                screening.ScreeningId,
                "Primary inhibition assay",
                AssayType.Biochemical,
                "LRRK2",
                "Prototype protocol",
                "Percent",
                50m),
            timeout.Token);

        var created = action.Result.Should()
            .BeOfType<CreatedAtActionResult>()
            .Subject;

        var response = created.Value.Should()
            .BeOfType<AssayResponse>()
            .Subject;

        response.Status.Should()
            .Be(AssayStatus.Draft);

        await publisher.Received(1).PublishAsync(
            Arg.Any<AssayCreated>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Complete_AssayWithMeasurements_CreatesResults()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var screening =
            CreateScreening();

        var assay = new Assay
        {
            Id = Guid.NewGuid(),
            ScreeningId = screening.ScreeningId,
            Name = "Assay",
            AssayType = AssayType.Biochemical,
            MeasurementUnit = "Percent",
            ActivityThreshold = 50m,
            Status = AssayStatus.InProgress,
            QualityControlStatus = QualityControlStatus.Pending,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        var compoundId =
            screening.Results.Single().CompoundId;

        database.Context.Screenings.Add(screening);
        database.Context.Assays.Add(assay);
        database.Context.AssayMeasurements.AddRange(
            Measurement(assay.Id, compoundId, 1, 70m),
            Measurement(assay.Id, compoundId, 2, 72m),
            Measurement(assay.Id, compoundId, 3, 74m));

        await database.Context.SaveChangesAsync(
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        var publisher =
            Substitute.For<IEventPublisher>();

        var controller =
            CreateController(
                database.Context,
                publisher);

        var action = await controller.Complete(
            assay.Id,
            new CompleteAssayRequest(
                0.72m,
                QualityControlStatus.Passed,
                "QC passed"),
            timeout.Token);

        var ok = action.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = ok.Value.Should()
            .BeOfType<AssayResponse>()
            .Subject;

        response.Status.Should()
            .Be(AssayStatus.Completed);

        response.Results.Should()
            .ContainSingle();

        await publisher.Received(1).PublishAsync(
            Arg.Any<AssayCompleted>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    private static AssaysController CreateController(
        Data.AssayManagementDbContext dbContext,
        IEventPublisher publisher)
    {
        return new AssaysController(
            dbContext,
            new DeterministicAssayResultCalculator(),
            publisher,
            NullLogger<AssaysController>.Instance);
    }

    private static CompletedScreeningProjection CreateScreening()
    {
        var screening = new CompletedScreeningProjection
        {
            ScreeningId = Guid.NewGuid(),
            Name = "Screening",
            ScreeningMethod = "Virtual",
            CompletedAtUtc = DateTimeOffset.UtcNow,
            Status = ScreeningProjectionStatus.Completed,
            SourceUpdatedAtUtc = DateTimeOffset.UtcNow,
            SynchronizedAtUtc = DateTimeOffset.UtcNow
        };

        screening.Results.Add(
            new CompletedScreeningResultProjection
            {
                Id = Guid.NewGuid(),
                ScreeningId = screening.ScreeningId,
                CompoundId = Guid.NewGuid(),
                Score = 0.90m,
                Rank = 1,
                Selected = true
            });

        return screening;
    }

    private static AssayMeasurement Measurement(
        Guid assayId,
        Guid compoundId,
        int replicate,
        decimal value)
    {
        return new AssayMeasurement
        {
            Id = Guid.NewGuid(),
            AssayId = assayId,
            CompoundId = compoundId,
            MeasurementType =
                MeasurementType.PercentInhibition,
            Concentration = 1m,
            ConcentrationUnit = "uM",
            ReplicateNumber = replicate,
            Value = value,
            RecordedAtUtc =
                DateTimeOffset.UtcNow
        };
    }
}

