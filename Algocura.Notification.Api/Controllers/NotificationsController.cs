using Algocura.Notification.Api.Contracts;
using Algocura.Notification.Api.Data;
using Algocura.Notification.Api.Domain;
using Algocura.Notification.Api.Events;
using Algocura.Notification.Api.Mapping;
using Algocura.Notification.Api.Services;
using Algocura.Platform.Messaging.Publishing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Notification.Api.Controllers;

[ApiController]
[Route("api/v1/notifications")]
public sealed class NotificationsController(
    NotificationDbContext db,
    INotificationDispatcher dispatcher,
    IEventPublisher publisher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<NotificationResponse>>> GetAll(
        NotificationStatus? status,
        string? eventType,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = db.Notifications
            .AsNoTracking()
            .Include(x => x.Deliveries)
            .ThenInclude(x => x.Attempts)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(x => x.SourceEventType == eventType.Trim());

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(Math.Max(skip, 0))
            .Take(Math.Clamp(take, 1, 200))
            .ToListAsync(cancellationToken);

        return Ok(items.Select(x => x.ToResponse()).ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NotificationResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await LoadAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<NotificationResponse>> CreateManual(
        CreateManualNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var item = new Algocura.Notification.Api.Domain.Notification
        {
            Id = Guid.NewGuid(),
            SourceEventId = Guid.NewGuid(),
            SourceEventType = request.SourceEventType.Trim(),
            SourceService = request.SourceService.Trim(),
            Subject = request.Subject.Trim(),
            Body = request.Body.Trim(),
            Priority = request.Priority,
            Status = NotificationStatus.Pending,
            CorrelationId = request.CorrelationId?.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        item.Deliveries.Add(
            new NotificationDelivery
            {
                Id = Guid.NewGuid(),
                NotificationId = item.Id,
                Channel = request.Channel,
                Recipient = request.Recipient.Trim(),
                Status = DeliveryStatus.Pending,
                AttemptCount = 0,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });

        db.Notifications.Add(item);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.Id },
            item.ToResponse());
    }

    [HttpPost("{id:guid}/dispatch")]
    public async Task<ActionResult<NotificationResponse>> Dispatch(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await LoadAsync(id, cancellationToken);
        if (item is null) return NotFound();

        if (item.Status == NotificationStatus.Cancelled)
            return Conflict(new ProblemDetails
            {
                Title = "Notification is cancelled",
                Status = StatusCodes.Status409Conflict
            });

        item.Status = NotificationStatus.Processing;
        item.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        foreach (var delivery in item.Deliveries
                     .Where(x => x.Status != DeliveryStatus.Delivered))
        {
            var result = await dispatcher.DispatchAsync(
                item,
                delivery,
                cancellationToken);

            var attemptedAt = DateTimeOffset.UtcNow;
            var attemptNumber = delivery.AttemptCount + 1;

            delivery.AttemptCount = attemptNumber;
            delivery.UpdatedAtUtc = attemptedAt;
            delivery.ProviderMessageId = result.ProviderMessageId;
            delivery.FailureReason = result.ErrorMessage;
            delivery.Status = result.Succeeded
                ? DeliveryStatus.Delivered
                : DeliveryStatus.Failed;
            delivery.DeliveredAtUtc = result.Succeeded
                ? attemptedAt
                : null;

            delivery.Attempts.Add(
                new NotificationDeliveryAttempt
                {
                    Id = Guid.NewGuid(),
                    NotificationDeliveryId = delivery.Id,
                    AttemptNumber = attemptNumber,
                    Status = delivery.Status,
                    ErrorMessage = result.ErrorMessage,
                    ProviderMessageId = result.ProviderMessageId,
                    AttemptedAtUtc = attemptedAt
                });
        }

        var delivered = item.Deliveries.Count(
            x => x.Status == DeliveryStatus.Delivered);
        var failed = item.Deliveries.Count(
            x => x.Status == DeliveryStatus.Failed);

        item.Status = failed == 0
            ? NotificationStatus.Delivered
            : delivered > 0
                ? NotificationStatus.PartiallyDelivered
                : NotificationStatus.Failed;

        item.DeliveredAtUtc =
            item.Status == NotificationStatus.Delivered
                ? DateTimeOffset.UtcNow
                : null;

        item.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        if (failed == 0)
        {
            await publisher.PublishAsync(
                new NotificationDelivered(
                    Guid.NewGuid(),
                    DateTimeOffset.UtcNow,
                    item.Id,
                    item.SourceEventId,
                    item.SourceEventType,
                    delivered),
                item.CorrelationId,
                cancellationToken);
        }
        else
        {
            await publisher.PublishAsync(
                new NotificationDeliveryFailed(
                    Guid.NewGuid(),
                    DateTimeOffset.UtcNow,
                    item.Id,
                    item.SourceEventId,
                    item.SourceEventType,
                    failed),
                item.CorrelationId,
                cancellationToken);
        }

        return Ok(item.ToResponse());
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<NotificationResponse>> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await LoadAsync(id, cancellationToken);
        if (item is null) return NotFound();

        if (item.Status == NotificationStatus.Delivered)
            return Conflict(new ProblemDetails
            {
                Title = "Notification was delivered",
                Status = StatusCodes.Status409Conflict
            });

        item.Status = NotificationStatus.Cancelled;
        item.UpdatedAtUtc = DateTimeOffset.UtcNow;

        foreach (var delivery in item.Deliveries
                     .Where(x => x.Status == DeliveryStatus.Pending))
        {
            delivery.Status = DeliveryStatus.Skipped;
            delivery.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(item.ToResponse());
    }

    private Task<Algocura.Notification.Api.Domain.Notification?> LoadAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        db.Notifications
            .Include(x => x.Deliveries)
            .ThenInclude(x => x.Attempts)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
}
