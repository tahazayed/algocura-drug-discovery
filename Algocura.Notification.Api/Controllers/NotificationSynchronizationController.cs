using Algocura.Notification.Api.Data;
using Algocura.Notification.Api.Domain;
using Algocura.Notification.Api.Events;
using Algocura.Platform.Messaging.Publishing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Notification.Api.Controllers;

[Authorize(Roles = "Service,Administrator")]
[ApiController]
[Route("api/v1/notification/synchronization")]
public sealed class NotificationSynchronizationController(
    NotificationDbContext db,
    IEventPublisher publisher,
    ILogger<NotificationSynchronizationController> logger)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<NotificationSynchronizationResponse>>
        SynchronizeCompletedNotifications(
            CancellationToken cancellationToken = default)
    {
        var startedAtUtc =
            DateTimeOffset.UtcNow;

        // SQLite cannot translate ORDER BY for DateTimeOffset.
        // Retrieve the records first, then order them in memory.
        var items = (await db.Notifications
                .AsNoTracking()
                .Include(x => x.Deliveries)
                .Where(x =>
                    x.Status == NotificationStatus.Delivered ||
                    x.Status == NotificationStatus.PartiallyDelivered ||
                    x.Status == NotificationStatus.Failed)
                .ToListAsync(cancellationToken))
            .OrderBy(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .ToList();

        var published = 0;

        var failures =
            new List<NotificationSynchronizationFailure>();

        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var delivered =
                    item.Deliveries.Count(
                        x =>
                            x.Status ==
                            DeliveryStatus.Delivered);

                var failed =
                    item.Deliveries.Count(
                        x =>
                            x.Status ==
                            DeliveryStatus.Failed);

                await publisher.PublishAsync(
                    new NotificationSnapshot(
                        Guid.NewGuid(),
                        DateTimeOffset.UtcNow,
                        item.Id,
                        item.SourceEventId,
                        item.SourceEventType,
                        item.SourceService,
                        item.Subject,
                        item.Status.ToString(),
                        item.Priority.ToString(),
                        delivered,
                        failed),
                    item.CorrelationId,
                    cancellationToken);

                published++;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to synchronize notification " +
                    "{NotificationId}.",
                    item.Id);

                failures.Add(
                    new NotificationSynchronizationFailure(
                        item.Id,
                        item.SourceEventId,
                        exception.Message));
            }
        }

        var completedAtUtc =
            DateTimeOffset.UtcNow;

        return Ok(
            new NotificationSynchronizationResponse(
                items.Count,
                published,
                failures.Count,
                startedAtUtc,
                completedAtUtc,
                failures));
    }
}

public sealed record NotificationSynchronizationResponse(
    int TotalCompletedNotifications,
    int PublishedEvents,
    int FailedEvents,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    IReadOnlyCollection<NotificationSynchronizationFailure> Failures);

public sealed record NotificationSynchronizationFailure(
    Guid NotificationId,
    Guid SourceEventId,
    string Error);