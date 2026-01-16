using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Algocura.Platform.Auditing.Models;

internal sealed class PendingAuditEntry
{
    public required EntityEntry Entry { get; init; }

    public required string EntityType { get; init; }

    public required string Action { get; init; }

    public Dictionary<string, object?> PreviousValues { get; } =
        new(StringComparer.Ordinal);

    public Dictionary<string, object?> CurrentValues { get; } =
        new(StringComparer.Ordinal);

    public HashSet<string> ChangedProperties { get; } =
        new(StringComparer.Ordinal);

    public bool HasTemporaryProperties =>
        Entry.Properties.Any(property => property.IsTemporary);
}

