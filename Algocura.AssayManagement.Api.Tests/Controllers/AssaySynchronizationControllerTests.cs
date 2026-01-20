using Algocura.AssayManagement.Api.Controllers;
using Algocura.AssayManagement.Api.Domain;
using Algocura.AssayManagement.Api.Events;
using Algocura.AssayManagement.Api.Tests.Support;
using Algocura.Platform.Messaging.Publishing;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Algocura.AssayManagement.Api.Tests.Controllers;

public sealed class AssaySynchronizationControllerTests
{
    [Fact]
    public async Task SynchronizePublishedAssays_PublishesOnlyPublishedAssays()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var publishedScreening =
            CreateScreening();

        var completedScreening =
            CreateScreening();

        var publishedAssay =
            CreateAssay(
                publishedScreening.ScreeningId,
                AssayStatus.Published);

        var completedAssay =
            CreateAssay(
                completedScreening.ScreeningId,
                AssayStatus.Completed);

        database.Context.Screenings.AddRange(
            publishedScreening,
            completedScreening);

        database.Context.Assays.AddRange(
            publishedAssay,
            completedAssay);

        await database.Context.SaveChangesAsync(
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        var publisher =
            Substitute.For<IEventPublisher>();

        var controller =
            new AssaySynchronizationController(
                database.Context,
                publisher,
                NullLogger<
                    AssaySynchronizationController>.Instance);

        var action =
            await controller.SynchronizePublishedAssays(
                timeout.Token);

        var ok = action.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = ok.Value.Should()
            .BeOfType<AssaySynchronizationResponse>()
            .Subject;

        response.TotalPublishedAssays.Should()
            .Be(1);

        response.PublishedEvents.Should()
            .Be(1);

        response.FailedEvents.Should()
            .Be(0);

        await publisher.Received(1).PublishAsync(
            Arg.Is<AssaySnapshot>(
                integrationEvent =>
                    integrationEvent.AssayId ==
                    publishedAssay.Id &&
                    integrationEvent.ScreeningId ==
                    publishedScreening.ScreeningId),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeById_PublishedAssay_PublishesSnapshot()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var screening =
            CreateScreening();

        var assay =
            CreateAssay(
                screening.ScreeningId,
                AssayStatus.Published);

        database.Context.Screenings.Add(
            screening);

        database.Context.Assays.Add(
            assay);

        await database.Context.SaveChangesAsync(
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        var publisher =
            Substitute.For<IEventPublisher>();

        var controller =
            new AssaySynchronizationController(
                database.Context,
                publisher,
                NullLogger<
                    AssaySynchronizationController>.Instance);

        var action =
            await controller.SynchronizeById(
                assay.Id,
                timeout.Token);

        var ok = action.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = ok.Value.Should()
            .BeOfType<AssaySynchronizationItemResponse>()
            .Subject;

        response.AssayId.Should()
            .Be(assay.Id);

        response.ScreeningId.Should()
            .Be(screening.ScreeningId);

        response.AssayName.Should()
            .Be(assay.Name);

        await publisher.Received(1).PublishAsync(
            Arg.Is<AssaySnapshot>(
                integrationEvent =>
                    integrationEvent.AssayId ==
                    assay.Id),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SynchronizeById_CompletedAssay_ReturnsConflict()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var screening =
            CreateScreening();

        var assay =
            CreateAssay(
                screening.ScreeningId,
                AssayStatus.Completed);

        database.Context.Screenings.Add(
            screening);

        database.Context.Assays.Add(
            assay);

        await database.Context.SaveChangesAsync(
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        var publisher =
            Substitute.For<IEventPublisher>();

        var controller =
            new AssaySynchronizationController(
                database.Context,
                publisher,
                NullLogger<
                    AssaySynchronizationController>.Instance);

        var action =
            await controller.SynchronizeById(
                assay.Id,
                timeout.Token);

        action.Result.Should()
            .BeOfType<ConflictObjectResult>();

        await publisher.DidNotReceive().PublishAsync(
            Arg.Any<AssaySnapshot>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    private static CompletedScreeningProjection CreateScreening()
    {
        var now =
            DateTimeOffset.UtcNow;

        return new CompletedScreeningProjection
        {
            ScreeningId =
                Guid.NewGuid(),

            Name =
                "Completed screening",

            ScreeningMethod =
                "Virtual",

            CompletedAtUtc =
                now,

            Status =
                ScreeningProjectionStatus.Completed,

            SourceUpdatedAtUtc =
                now,

            SynchronizedAtUtc =
                now
        };
    }

    private static Assay CreateAssay(
        Guid screeningId,
        AssayStatus status)
    {
        var now =
            DateTimeOffset.UtcNow;

        return new Assay
        {
            Id =
                Guid.NewGuid(),

            ScreeningId =
                screeningId,

            Name =
                "Assay",

            AssayType =
                AssayType.Biochemical,

            BiologicalTarget =
                "LRRK2",

            Protocol =
                "Prototype assay protocol",

            MeasurementUnit =
                "Percent",

            ActivityThreshold =
                50m,

            Status =
                status,

            QualityControlStatus =
                QualityControlStatus.Passed,

            ZPrimeFactor =
                0.72m,

            QualityControlNotes =
                "Quality control passed.",

            CreatedAtUtc =
                now,

            UpdatedAtUtc =
                now,

            CompletedAtUtc =
                now,

            PublishedAtUtc =
                status == AssayStatus.Published
                    ? now
                    : null
        };
    }
}