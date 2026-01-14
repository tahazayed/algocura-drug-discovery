using Algocura.Notification.Api.Controllers;
using Algocura.Notification.Api.Domain;
using Algocura.Notification.Api.Events;
using Algocura.Notification.Api.Messaging.Publishers;
using Algocura.Notification.Api.Services;
using Algocura.Notification.Api.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Algocura.Notification.Api.Tests.Controllers;

public sealed class NotificationsControllerTests
{
    [Fact]
    public async Task Dispatch_PendingNotification_MarksDelivered()
    {
        await using var database = TestDatabase.Create();
        using var timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var now = DateTimeOffset.UtcNow;
        var notification = new Algocura.Notification.Api.Domain.Notification
        {
            Id = Guid.NewGuid(),
            SourceEventId = Guid.NewGuid(),
            SourceEventType = "AssayCompleted",
            SourceService = "Algocura.AssayManagement.Api",
            Subject = "Assay completed",
            Body = "Body",
            Priority = NotificationPriority.High,
            Status = NotificationStatus.Pending,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        notification.Deliveries.Add(
            new NotificationDelivery
            {
                Id = Guid.NewGuid(),
                NotificationId = notification.Id,
                Channel = NotificationChannel.InApplication,
                Recipient = "scientists",
                Status = DeliveryStatus.Pending,
                AttemptCount = 0,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });

        database.Context.Notifications.Add(notification);
        await database.Context.SaveChangesAsync(timeout.Token);
        database.Context.ChangeTracker.Clear();

        var dispatcher = Substitute.For<INotificationDispatcher>();
        dispatcher.DispatchAsync(
                Arg.Any<Algocura.Notification.Api.Domain.Notification>(),
                Arg.Any<Algocura.Notification.Api.Domain.NotificationDelivery>(),
                Arg.Any<CancellationToken>())
            .Returns(new NotificationDispatchResult(true, "provider-1", null));

        var publisher = Substitute.For<IEventPublisher>();

        var controller = new NotificationsController(
            database.Context,
            dispatcher,
            publisher);

        var action = await controller.Dispatch(
            notification.Id,
            timeout.Token);

        var ok = action.Result.Should()
            .BeOfType<OkObjectResult>().Subject;

        var response = ok.Value.Should()
            .BeOfType<Contracts.NotificationResponse>().Subject;

        response.Status.Should().Be(NotificationStatus.Delivered);

        await publisher.Received(1).PublishAsync(
            Arg.Any<NotificationDelivered>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}
