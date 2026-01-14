using System.ComponentModel.DataAnnotations;

namespace Algocura.Notification.Api.Domain;

public enum NotificationChannel { Email = 1, MicrosoftTeams = 2, Webhook = 3, InApplication = 4 }
public enum NotificationPriority { Low = 1, Normal = 2, High = 3, Critical = 4 }
public enum NotificationStatus { Pending = 1, Processing = 2, Delivered = 3, PartiallyDelivered = 4, Failed = 5, Cancelled = 6 }
public enum DeliveryStatus { Pending = 1, Delivered = 2, Failed = 3, Skipped = 4 }

public sealed class NotificationRule
{
    public Guid Id { get; set; }
    [MaxLength(200)] public required string Name { get; set; }
    [MaxLength(200)] public required string EventType { get; set; }
    public NotificationChannel Channel { get; set; }
    [MaxLength(500)] public required string Recipient { get; set; }
    [MaxLength(300)] public required string SubjectTemplate { get; set; }
    [MaxLength(4000)] public required string BodyTemplate { get; set; }
    public NotificationPriority Priority { get; set; }
    public bool Enabled { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public List<Algocura.Notification.Api.Domain.Notification> Notifications { get; set; } = [];
}

public sealed class Notification
{
    public Guid Id { get; set; }
    public Guid? RuleId { get; set; }
    public Guid SourceEventId { get; set; }
    [MaxLength(200)] public required string SourceEventType { get; set; }
    [MaxLength(200)] public required string SourceService { get; set; }
    [MaxLength(300)] public required string Subject { get; set; }
    [MaxLength(4000)] public required string Body { get; set; }
    public NotificationPriority Priority { get; set; }
    public NotificationStatus Status { get; set; }
    [MaxLength(100)] public string? CorrelationId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public DateTimeOffset? DeliveredAtUtc { get; set; }
    public NotificationRule? Rule { get; set; }
    public List<NotificationDelivery> Deliveries { get; set; } = [];
}

public sealed class NotificationDelivery
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public NotificationChannel Channel { get; set; }
    [MaxLength(500)] public required string Recipient { get; set; }
    public DeliveryStatus Status { get; set; }
    public int AttemptCount { get; set; }
    [MaxLength(2000)] public string? FailureReason { get; set; }
    [MaxLength(200)] public string? ProviderMessageId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public DateTimeOffset? DeliveredAtUtc { get; set; }
    public Notification? Notification { get; set; }
    public List<NotificationDeliveryAttempt> Attempts { get; set; } = [];
}

public sealed class NotificationDeliveryAttempt
{
    public Guid Id { get; set; }
    public Guid NotificationDeliveryId { get; set; }
    public int AttemptNumber { get; set; }
    public DeliveryStatus Status { get; set; }
    [MaxLength(2000)] public string? ErrorMessage { get; set; }
    [MaxLength(200)] public string? ProviderMessageId { get; set; }
    public DateTimeOffset AttemptedAtUtc { get; set; }
    public NotificationDelivery? NotificationDelivery { get; set; }
}
