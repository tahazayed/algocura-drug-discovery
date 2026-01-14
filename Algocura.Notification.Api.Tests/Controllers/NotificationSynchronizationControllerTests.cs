using Algocura.Notification.Api.Controllers;
using Algocura.Notification.Api.Domain;
using Algocura.Notification.Api.Events;
using Algocura.Notification.Api.Messaging.Publishers;
using Algocura.Notification.Api.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Algocura.Notification.Api.Tests.Controllers;

public sealed class NotificationSynchronizationControllerTests
{
    [Fact]
    public async Task SynchronizeCompletedNotifications_PublishesCompletedOnly()
    {
        await using var database = TestDatabase.Create();
        using var timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(10));

        database.Context.Notifications.AddRange(
            Create(NotificationStatus.Delivered),
            Create(NotificationStatus.Pending));

        await database.Context.SaveChangesAsync(timeout.Token);
        database.Context.ChangeTracker.Clear();

        var publisher = Substitute.For<IEventPublisher>();

        var controller = new NotificationSynchronizationController(
            database.Context,
            publisher,
            NullLogger<NotificationSynchronizationController>.Instance);

        var action = await controller.SynchronizeCompletedNotifications(
            timeout.Token);

        var ok = action.Result.Should()
            .BeOfType<OkObjectResult>().Subject;

        var response = ok.Value.Should()
            .BeOfType<NotificationSynchronizationResponse>().Subject;

        response.TotalCompletedNotifications.Should().Be(1);
        response.PublishedEvents.Should().Be(1);

        await publisher.Received(1).PublishAsync(
            Arg.Any<NotificationSnapshot>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    private static Algocura.Notification.Api.Domain.Notification Create(NotificationStatus status) =>
        new()
        {
            Id = Guid.NewGuid(),
            SourceEventId = Guid.NewGuid(),
            SourceEventType = "AssayCompleted",
            SourceService = "Algocura.AssayManagement.Api",
            Subject = "Assay completed",
            Body = "Body",
            Priority = NotificationPriority.High,
            Status = status,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            DeliveredAtUtc = status == NotificationStatus.Delivered
                ? DateTimeOffset.UtcNow
                : null
        };
}
