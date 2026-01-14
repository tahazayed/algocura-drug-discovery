using Algocura.Notification.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Notification.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        const int maximumAttempts = 10;

        for (var attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            try
            {
                await using var scope = services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

                await db.Database.MigrateAsync(cancellationToken);

                if (!await db.NotificationRules.AnyAsync(cancellationToken))
                {
                    var now = DateTimeOffset.UtcNow;

                    db.NotificationRules.AddRange(
                        new NotificationRule
                        {
                            Id = Guid.NewGuid(),
                            Name = "Assay Completion Notification",
                            EventType = "AssayCompleted",
                            Channel = NotificationChannel.InApplication,
                            Recipient = "discovery-team",
                            SubjectTemplate = "{{eventType}} received",
                            BodyTemplate = "Event {{eventId}} from {{source}} was received.",
                            Priority = NotificationPriority.High,
                            Enabled = true,
                            CreatedAtUtc = now,
                            UpdatedAtUtc = now
                        },
                        new NotificationRule
                        {
                            Id = Guid.NewGuid(),
                            Name = "Target Selection Notification",
                            EventType = "TargetSelected",
                            Channel = NotificationChannel.InApplication,
                            Recipient = "discovery-team",
                            SubjectTemplate = "Target selected",
                            BodyTemplate = "Target selection event {{eventId}} was received.",
                            Priority = NotificationPriority.Normal,
                            Enabled = true,
                            CreatedAtUtc = now,
                            UpdatedAtUtc = now
                        });

                    await db.SaveChangesAsync(cancellationToken);
                }

                logger.LogInformation("Notification database initialization completed.");
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception) when (attempt < maximumAttempts)
            {
                var delay = TimeSpan.FromSeconds(attempt * 2);
                logger.LogWarning(
                    exception,
                    "Notification initialization attempt {Attempt} of {MaximumAttempts} failed. Retrying in {DelaySeconds} seconds.",
                    attempt,
                    maximumAttempts,
                    delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }
        }
    }
}
