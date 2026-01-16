using Algocura.Platform.Auditing.Abstractions;
using Algocura.Platform.Auditing.Events;
using Algocura.Platform.Auditing.Models;
using Algocura.Platform.Auditing.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.Platform.Auditing.Interceptors;

public sealed class EntityAuditEventInterceptor(
    ICurrentUserContext currentUser,
    IAuditEventPublisher auditEventPublisher,
    IAuditValueSanitizer sanitizer,
    IOptions<AuditingOptions> options)
    : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly AuditingOptions _options = options.Value;
    private readonly List<PendingAuditEntry> _pendingEntries = [];

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        CaptureChanges(eventData.Context);

        return base.SavingChanges(
            eventData,
            result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CaptureChanges(eventData.Context);

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        PublishPendingEventsAsync(
                eventData.Context,
                CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        return base.SavedChanges(
            eventData,
            result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await PublishPendingEventsAsync(
            eventData.Context,
            cancellationToken);

        return await base.SavedChangesAsync(
            eventData,
            result,
            cancellationToken);
    }

    public override void SaveChangesFailed(
        DbContextErrorEventData eventData)
    {
        _pendingEntries.Clear();

        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _pendingEntries.Clear();

        return base.SaveChangesFailedAsync(
            eventData,
            cancellationToken);
    }

    private void CaptureChanges(DbContext? dbContext)
    {
        _pendingEntries.Clear();

        if (dbContext is null ||
            !_options.PublishEntityAuditEvents)
        {
            return;
        }

        dbContext.ChangeTracker.DetectChanges();

        foreach (var entry in dbContext.ChangeTracker.Entries())
        {
            if (entry.State is
                EntityState.Detached or
                EntityState.Unchanged)
            {
                continue;
            }

            var entityType =
                entry.Metadata.ClrType.Name;

            if (_options.ExcludedEntityTypes.Contains(entityType))
            {
                continue;
            }

            if (_options.IgnoreAuditInfrastructureEntities &&
                entityType.Contains(
                    "Audit",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var pending =
                new PendingAuditEntry
                {
                    Entry = entry,
                    EntityType = entityType,
                    Action = MapAction(entry.State)
                };

            CapturePropertyValues(
                entry,
                pending);

            _pendingEntries.Add(pending);
        }
    }

    private void CapturePropertyValues(
        EntityEntry entry,
        PendingAuditEntry pending)
    {
        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsShadowProperty())
            {
                continue;
            }

            var propertyName =
                property.Metadata.Name;

            if (property.IsTemporary)
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    pending.CurrentValues[propertyName] =
                        property.CurrentValue;

                    if (_options.IncludeUnchangedPropertiesOnCreate)
                    {
                        pending.ChangedProperties.Add(
                            propertyName);
                    }

                    break;

                case EntityState.Modified:
                    if (!property.IsModified)
                    {
                        continue;
                    }

                    pending.PreviousValues[propertyName] =
                        property.OriginalValue;

                    pending.CurrentValues[propertyName] =
                        property.CurrentValue;

                    pending.ChangedProperties.Add(
                        propertyName);

                    break;

                case EntityState.Deleted:
                    pending.PreviousValues[propertyName] =
                        property.OriginalValue;

                    if (_options.IncludeUnchangedPropertiesOnDelete)
                    {
                        pending.ChangedProperties.Add(
                            propertyName);
                    }

                    break;
            }
        }
    }

    private async Task PublishPendingEventsAsync(
        DbContext? dbContext,
        CancellationToken cancellationToken)
    {
        if (dbContext is null ||
            _pendingEntries.Count == 0)
        {
            return;
        }

        var entries =
            _pendingEntries.ToArray();

        _pendingEntries.Clear();

        foreach (var pending in entries)
        {
            PopulateTemporaryValues(pending);

            var entityId =
                ResolveEntityId(
                    pending.Entry);

            var sanitizedPrevious =
                sanitizer.Sanitize(
                    pending.EntityType,
                    pending.PreviousValues);

            var sanitizedCurrent =
                sanitizer.Sanitize(
                    pending.EntityType,
                    pending.CurrentValues);

            var changedProperties =
                pending.ChangedProperties
                    .Where(
                        property =>
                            !_options.ExcludedPropertyNames.Contains(
                                property))
                    .OrderBy(property => property)
                    .ToArray();

            var auditEvent =
                new EntityAuditEvent
                {
                    EventId = Guid.NewGuid(),
                    OccurredAtUtc =
                        DateTimeOffset.UtcNow,
                    ServiceName =
                        _options.ServiceName,
                    EntityType =
                        pending.EntityType,
                    EntityId =
                        entityId,
                    Action =
                        pending.Action,
                    UserId =
                        currentUser.UserId,
                    Subject =
                        currentUser.Subject,
                    UserDisplayName =
                        currentUser.DisplayName,
                    UserEmail =
                        currentUser.Email,
                    ActorType =
                        currentUser.ActorType,
                    PreviousValuesJson =
                        sanitizedPrevious.Count == 0
                            ? null
                            : JsonSerializer.Serialize(
                                sanitizedPrevious,
                                JsonOptions),
                    CurrentValuesJson =
                        sanitizedCurrent.Count == 0
                            ? null
                            : JsonSerializer.Serialize(
                                sanitizedCurrent,
                                JsonOptions),
                    ChangedPropertiesJson =
                        changedProperties.Length == 0
                            ? null
                            : JsonSerializer.Serialize(
                                changedProperties,
                                JsonOptions),
                    CorrelationId =
                        currentUser.CorrelationId,
                    TraceId =
                        currentUser.TraceId,
                    IpAddress =
                        currentUser.IpAddress,
                    UserAgent =
                        currentUser.UserAgent
                };

            await auditEventPublisher.PublishAsync(
                auditEvent,
                cancellationToken);
        }
    }

    private static void PopulateTemporaryValues(
        PendingAuditEntry pending)
    {
        foreach (var property in pending.Entry.Properties)
        {
            if (!property.IsTemporary)
            {
                continue;
            }

            pending.CurrentValues[property.Metadata.Name] =
                property.CurrentValue;
        }
    }

    private static string ResolveEntityId(
        EntityEntry entry)
    {
        var primaryKey =
            entry.Metadata.FindPrimaryKey();

        if (primaryKey is null)
        {
            return string.Empty;
        }

        var values =
            primaryKey.Properties
                .Select(
                    property =>
                        entry.Property(property.Name)
                            .CurrentValue
                            ?.ToString()
                        ?? string.Empty)
                .ToArray();

        return string.Join(
            "|",
            values);
    }

    private static string MapAction(
        EntityState state)
    {
        return state switch
        {
            EntityState.Added =>
                AuditAction.Created,

            EntityState.Modified =>
                AuditAction.Updated,

            EntityState.Deleted =>
                AuditAction.Deleted,

            _ =>
                state.ToString()
        };
    }
}
