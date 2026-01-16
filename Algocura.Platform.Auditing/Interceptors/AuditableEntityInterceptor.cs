using Algocura.Platform.Auditing.Abstractions;
using Algocura.Platform.Auditing.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace Algocura.Platform.Auditing.Interceptors;

public sealed class AuditableEntityInterceptor(
    ICurrentUserContext currentUser,
    IOptions<AuditingOptions> options)
    : SaveChangesInterceptor
{
    private readonly AuditingOptions _options = options.Value;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        StampEntities(eventData.Context);

        return base.SavingChanges(
            eventData,
            result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        StampEntities(eventData.Context);

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }

    private void StampEntities(DbContext? dbContext)
    {
        if (dbContext is null ||
            !_options.StampCreatedAndUpdatedFields)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var userId = currentUser.UserId;

        foreach (var entry in dbContext.ChangeTracker
                     .Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAtUtc == default)
                {
                    entry.Entity.CreatedAtUtc = now;
                }

                entry.Entity.UpdatedAtUtc = now;

                if (entry.Entity.CreatedByUserId is null)
                {
                    entry.Entity.CreatedByUserId = userId;
                }

                entry.Entity.UpdatedByUserId = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = now;
                entry.Entity.UpdatedByUserId = userId;

                entry.Property(
                        nameof(IAuditableEntity.CreatedAtUtc))
                    .IsModified = false;

                entry.Property(
                        nameof(IAuditableEntity.CreatedByUserId))
                    .IsModified = false;
            }
        }
    }
}
