using Algocura.Notification.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Notification.Api.Data;

public static class NotificationSeeder
{
    private static readonly DateTimeOffset SeedTimestamp =
        new(
            2026,
            1,
            1,
            0,
            0,
            0,
            TimeSpan.Zero);

    private static readonly Guid AssayCompletionRuleId =
        Guid.Parse("81000000-0000-0000-0000-000000000001");

    private static readonly Guid TargetSelectionRuleId =
        Guid.Parse("81000000-0000-0000-0000-000000000002");

    private static readonly Guid WorkflowCompletionRuleId =
        Guid.Parse("81000000-0000-0000-0000-000000000003");

    private static readonly Guid AssayNotificationId =
        Guid.Parse("82000000-0000-0000-0000-000000000001");

    private static readonly Guid TargetNotificationId =
        Guid.Parse("82000000-0000-0000-0000-000000000002");

    private static readonly Guid WorkflowNotificationId =
        Guid.Parse("82000000-0000-0000-0000-000000000003");

    private static readonly Guid AssayDeliveryId =
        Guid.Parse("83000000-0000-0000-0000-000000000001");

    private static readonly Guid TargetDeliveryId =
        Guid.Parse("83000000-0000-0000-0000-000000000002");

    private static readonly Guid WorkflowDeliveryId =
        Guid.Parse("83000000-0000-0000-0000-000000000003");

    private static readonly Guid AssayAttemptId =
        Guid.Parse("84000000-0000-0000-0000-000000000001");

    private static readonly Guid TargetAttemptId =
        Guid.Parse("84000000-0000-0000-0000-000000000002");

    private static readonly Guid WorkflowAttemptId =
        Guid.Parse("84000000-0000-0000-0000-000000000003");

    // Event identifiers are deterministic prototype values.
    private static readonly Guid AssayCompletedEventId =
        Guid.Parse("85000000-0000-0000-0000-000000000001");

    private static readonly Guid TargetSelectedEventId =
        Guid.Parse("85000000-0000-0000-0000-000000000002");

    private static readonly Guid WorkflowCompletedEventId =
        Guid.Parse("85000000-0000-0000-0000-000000000003");

    public static async Task SeedAsync(
        NotificationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        await SeedRulesAsync(
            dbContext,
            cancellationToken);

        await SeedNotificationsAsync(
            dbContext,
            cancellationToken);

        await SeedDeliveriesAsync(
            dbContext,
            cancellationToken);

        await SeedDeliveryAttemptsAsync(
            dbContext,
            cancellationToken);
    }

    private static async Task SeedRulesAsync(
        NotificationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var rules =
            new[]
            {
                new NotificationRule
                {
                    Id =
                        AssayCompletionRuleId,

                    Name =
                        "Assay Completion Notification",

                    EventType =
                        "AssayCompleted",

                    Channel =
                        NotificationChannel.InApplication,

                    Recipient =
                        "discovery-team",

                    SubjectTemplate =
                        "{{eventType}} received",

                    BodyTemplate =
                        "Event {{eventId}} from {{source}} was received.",

                    Priority =
                        NotificationPriority.High,

                    Enabled =
                        true,

                    OwnerUserId =
                        SystemUsers.SystemUserId,

                    CreatedByUserId =
                        SystemUsers.SystemUserId,

                    UpdatedByUserId =
                        SystemUsers.SystemUserId,

                    CreatedAtUtc =
                        SeedTimestamp,

                    UpdatedAtUtc =
                        SeedTimestamp
                },

                new NotificationRule
                {
                    Id =
                        TargetSelectionRuleId,

                    Name =
                        "Target Selection Notification",

                    EventType =
                        "TargetSelected",

                    Channel =
                        NotificationChannel.InApplication,

                    Recipient =
                        "discovery-team",

                    SubjectTemplate =
                        "Target selected",

                    BodyTemplate =
                        "Target selection event {{eventId}} was received.",

                    Priority =
                        NotificationPriority.Normal,

                    Enabled =
                        true,

                    OwnerUserId =
                        SystemUsers.SystemUserId,

                    CreatedByUserId =
                        SystemUsers.SystemUserId,

                    UpdatedByUserId =
                        SystemUsers.SystemUserId,

                    CreatedAtUtc =
                        SeedTimestamp,

                    UpdatedAtUtc =
                        SeedTimestamp
                },

                new NotificationRule
                {
                    Id =
                        WorkflowCompletionRuleId,

                    Name =
                        "Workflow Completion Notification",

                    EventType =
                        "WorkflowCompleted",

                    Channel =
                        NotificationChannel.InApplication,

                    Recipient =
                        "discovery-team",

                    SubjectTemplate =
                        "Discovery workflow completed",

                    BodyTemplate =
                        "Workflow completion event {{eventId}} was received.",

                    Priority =
                        NotificationPriority.High,

                    Enabled =
                        true,

                    OwnerUserId =
                        SystemUsers.SystemUserId,

                    CreatedByUserId =
                        SystemUsers.SystemUserId,

                    UpdatedByUserId =
                        SystemUsers.SystemUserId,

                    CreatedAtUtc =
                        SeedTimestamp,

                    UpdatedAtUtc =
                        SeedTimestamp
                }
            };

        var seedIds =
            rules
                .Select(rule => rule.Id)
                .ToArray();

        var existingIds =
            await dbContext.NotificationRules
                .AsNoTracking()
                .Where(rule =>
                    seedIds.Contains(rule.Id))
                .Select(rule => rule.Id)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var missingRules =
            rules
                .Where(rule =>
                    !existingIdSet.Contains(rule.Id))
                .ToArray();

        if (missingRules.Length == 0)
        {
            return;
        }

        await dbContext.NotificationRules.AddRangeAsync(
            missingRules,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedNotificationsAsync(
        NotificationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var notifications =
            new[]
            {
                CreateNotification(
                    AssayNotificationId,
                    AssayCompletionRuleId,
                    AssayCompletedEventId,
                    "AssayCompleted",
                    "Algocura.AssayManagement.Api",
                    "Assay completed",
                    "The prototype TREM2 functional assay completed successfully.",
                    NotificationPriority.High,
                    "assay-completed-correlation"),

                CreateNotification(
                    TargetNotificationId,
                    TargetSelectionRuleId,
                    TargetSelectedEventId,
                    "TargetSelected",
                    "Algocura.TargetDiscovery.Api",
                    "Target selected",
                    "The TREM2 target assessment was selected for chemical design.",
                    NotificationPriority.Normal,
                    "target-selected-correlation"),

                CreateNotification(
                    WorkflowNotificationId,
                    WorkflowCompletionRuleId,
                    WorkflowCompletedEventId,
                    "WorkflowCompleted",
                    "Algocura.Workflow.Api",
                    "Discovery workflow completed",
                    "The prototype TREM2 discovery workflow completed successfully.",
                    NotificationPriority.High,
                    "workflow-completed-correlation")
            };

        var seedIds =
            notifications
                .Select(notification => notification.Id)
                .ToArray();

        var existingIds =
            await dbContext.Notifications
                .AsNoTracking()
                .Where(notification =>
                    seedIds.Contains(notification.Id))
                .Select(notification => notification.Id)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var missingNotifications =
            notifications
                .Where(notification =>
                    !existingIdSet.Contains(notification.Id))
                .ToArray();

        if (missingNotifications.Length == 0)
        {
            return;
        }

        await dbContext.Notifications.AddRangeAsync(
            missingNotifications,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedDeliveriesAsync(
        NotificationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var deliveries =
            new[]
            {
                CreateDelivery(
                    AssayDeliveryId,
                    AssayNotificationId,
                    "assay-delivery-0001"),

                CreateDelivery(
                    TargetDeliveryId,
                    TargetNotificationId,
                    "target-delivery-0001"),

                CreateDelivery(
                    WorkflowDeliveryId,
                    WorkflowNotificationId,
                    "workflow-delivery-0001")
            };

        var seedIds =
            deliveries
                .Select(delivery => delivery.Id)
                .ToArray();

        var existingIds =
            await dbContext.NotificationDeliveries
                .AsNoTracking()
                .Where(delivery =>
                    seedIds.Contains(delivery.Id))
                .Select(delivery => delivery.Id)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var missingDeliveries =
            deliveries
                .Where(delivery =>
                    !existingIdSet.Contains(delivery.Id))
                .ToArray();

        if (missingDeliveries.Length == 0)
        {
            return;
        }

        await dbContext.NotificationDeliveries.AddRangeAsync(
            missingDeliveries,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedDeliveryAttemptsAsync(
        NotificationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var attempts =
            new[]
            {
                CreateAttempt(
                    AssayAttemptId,
                    AssayDeliveryId,
                    "assay-delivery-0001"),

                CreateAttempt(
                    TargetAttemptId,
                    TargetDeliveryId,
                    "target-delivery-0001"),

                CreateAttempt(
                    WorkflowAttemptId,
                    WorkflowDeliveryId,
                    "workflow-delivery-0001")
            };

        var seedIds =
            attempts
                .Select(attempt => attempt.Id)
                .ToArray();

        var existingIds =
            await dbContext.NotificationDeliveryAttempts
                .AsNoTracking()
                .Where(attempt =>
                    seedIds.Contains(attempt.Id))
                .Select(attempt => attempt.Id)
                .ToListAsync(cancellationToken);

        var existingIdSet =
            existingIds.ToHashSet();

        var missingAttempts =
            attempts
                .Where(attempt =>
                    !existingIdSet.Contains(attempt.Id))
                .ToArray();

        if (missingAttempts.Length == 0)
        {
            return;
        }

        await dbContext.NotificationDeliveryAttempts.AddRangeAsync(
            missingAttempts,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static Algocura.Notification.Api.Domain.Notification CreateNotification(
        Guid id,
        Guid ruleId,
        Guid sourceEventId,
        string sourceEventType,
        string sourceService,
        string subject,
        string body,
        NotificationPriority priority,
        string correlationId)
    {
        return new Algocura.Notification.Api.Domain.Notification
        {
            Id =
                id,

            RuleId =
                ruleId,

            SourceEventId =
                sourceEventId,

            SourceEventType =
                sourceEventType,

            SourceService =
                sourceService,

            Subject =
                subject,

            Body =
                body,

            Priority =
                priority,

            Status =
                NotificationStatus.Delivered,

            CorrelationId =
                correlationId,

            SourceCreatedByUserId =
                SystemUsers.SystemUserId,

            SourceUpdatedByUserId =
                SystemUsers.SystemUserId,

            CancelledByUserId =
                null,

            CancelledAtUtc =
                null,

            CreatedByUserId =
                SystemUsers.SystemUserId,

            UpdatedByUserId =
                SystemUsers.SystemUserId,

            CreatedAtUtc =
                SeedTimestamp,

            UpdatedAtUtc =
                SeedTimestamp,

            DeliveredAtUtc =
                SeedTimestamp
        };
    }

    private static NotificationDelivery CreateDelivery(
        Guid id,
        Guid notificationId,
        string providerMessageId)
    {
        return new NotificationDelivery
        {
            Id =
                id,

            NotificationId =
                notificationId,

            Channel =
                NotificationChannel.InApplication,

            Recipient =
                "discovery-team",

            Status =
                DeliveryStatus.Delivered,

            AttemptCount =
                1,

            FailureReason =
                null,

            ProviderMessageId =
                providerMessageId,

            CompletedByUserId =
                SystemUsers.SystemUserId,

            CompletedAtUtc =
                SeedTimestamp,

            DeliveredAtUtc =
                SeedTimestamp,

            CreatedByUserId =
                SystemUsers.SystemUserId,

            UpdatedByUserId =
                SystemUsers.SystemUserId,

            CreatedAtUtc =
                SeedTimestamp,

            UpdatedAtUtc =
                SeedTimestamp
        };
    }

    private static NotificationDeliveryAttempt CreateAttempt(
        Guid id,
        Guid deliveryId,
        string providerMessageId)
    {
        return new NotificationDeliveryAttempt
        {
            Id =
                id,

            NotificationDeliveryId =
                deliveryId,

            AttemptNumber =
                1,

            Status =
                DeliveryStatus.Delivered,

            ErrorMessage =
                null,

            ProviderMessageId =
                providerMessageId,

            AttemptedAtUtc =
                SeedTimestamp,

            CreatedByUserId =
                SystemUsers.SystemUserId,

            UpdatedByUserId =
                SystemUsers.SystemUserId,

            CreatedAtUtc =
                SeedTimestamp,

            UpdatedAtUtc =
                SeedTimestamp
        };
    }
}
