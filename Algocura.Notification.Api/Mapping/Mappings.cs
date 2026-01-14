using Algocura.Notification.Api.Contracts;
using Algocura.Notification.Api.Domain;

namespace Algocura.Notification.Api.Mapping;

public static class Mappings
{
    public static NotificationRuleResponse ToResponse(this NotificationRule x) =>
        new(x.Id, x.Name, x.EventType, x.Channel, x.Recipient,
            x.SubjectTemplate, x.BodyTemplate, x.Priority, x.Enabled,
            x.CreatedAtUtc, x.UpdatedAtUtc);

    public static NotificationResponse ToResponse(this Algocura.Notification.Api.Domain.Notification x) =>
        new(x.Id, x.RuleId, x.SourceEventId, x.SourceEventType,
            x.SourceService, x.Subject, x.Body, x.Priority, x.Status,
            x.CorrelationId, x.CreatedAtUtc, x.UpdatedAtUtc, x.DeliveredAtUtc,
            x.Deliveries.OrderBy(d => d.CreatedAtUtc)
                .Select(d => new NotificationDeliveryResponse(
                    d.Id, d.Channel, d.Recipient, d.Status, d.AttemptCount,
                    d.FailureReason, d.ProviderMessageId, d.CreatedAtUtc,
                    d.UpdatedAtUtc, d.DeliveredAtUtc,
                    d.Attempts.OrderBy(a => a.AttemptNumber)
                        .Select(a => new NotificationDeliveryAttemptResponse(
                            a.Id, a.AttemptNumber, a.Status, a.ErrorMessage,
                            a.ProviderMessageId, a.AttemptedAtUtc))
                        .ToArray()))
                .ToArray());
}
