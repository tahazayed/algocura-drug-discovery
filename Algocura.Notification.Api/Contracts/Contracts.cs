using Algocura.Notification.Api.Domain;
using System.ComponentModel.DataAnnotations;

namespace Algocura.Notification.Api.Contracts;

public sealed record CreateNotificationRuleRequest(
    [property: Required, MaxLength(200)] string Name,
    [property: Required, MaxLength(200)] string EventType,
    NotificationChannel Channel,
    [property: Required, MaxLength(500)] string Recipient,
    [property: Required, MaxLength(300)] string SubjectTemplate,
    [property: Required, MaxLength(4000)] string BodyTemplate,
    NotificationPriority Priority,
    bool Enabled = true);

public sealed record UpdateNotificationRuleRequest(
    [property: Required, MaxLength(200)] string Name,
    [property: Required, MaxLength(200)] string EventType,
    NotificationChannel Channel,
    [property: Required, MaxLength(500)] string Recipient,
    [property: Required, MaxLength(300)] string SubjectTemplate,
    [property: Required, MaxLength(4000)] string BodyTemplate,
    NotificationPriority Priority,
    bool Enabled);

public sealed record CreateManualNotificationRequest(
    [property: Required, MaxLength(200)] string SourceEventType,
    [property: Required, MaxLength(200)] string SourceService,
    [property: Required, MaxLength(300)] string Subject,
    [property: Required, MaxLength(4000)] string Body,
    NotificationPriority Priority,
    NotificationChannel Channel,
    [property: Required, MaxLength(500)] string Recipient,
    [property: MaxLength(100)] string? CorrelationId);

public sealed record NotificationRuleResponse(
    Guid Id, string Name, string EventType, NotificationChannel Channel,
    string Recipient, string SubjectTemplate, string BodyTemplate,
    NotificationPriority Priority, bool Enabled,
    DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc);

public sealed record NotificationDeliveryAttemptResponse(
    Guid Id, int AttemptNumber, DeliveryStatus Status,
    string? ErrorMessage, string? ProviderMessageId,
    DateTimeOffset AttemptedAtUtc);

public sealed record NotificationDeliveryResponse(
    Guid Id, NotificationChannel Channel, string Recipient,
    DeliveryStatus Status, int AttemptCount, string? FailureReason,
    string? ProviderMessageId, DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc, DateTimeOffset? DeliveredAtUtc,
    IReadOnlyCollection<NotificationDeliveryAttemptResponse> Attempts);

public sealed record NotificationResponse(
    Guid Id, Guid? RuleId, Guid SourceEventId, string SourceEventType,
    string SourceService, string Subject, string Body,
    NotificationPriority Priority, NotificationStatus Status,
    string? CorrelationId, DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc, DateTimeOffset? DeliveredAtUtc,
    IReadOnlyCollection<NotificationDeliveryResponse> Deliveries);
