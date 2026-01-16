namespace Algocura.Platform.Auditing.Abstractions;

/// <summary>
/// Defines standard creation and modification metadata for persisted entities.
/// </summary>
public interface IAuditableEntity
{
    Guid? CreatedByUserId { get; set; }

    Guid? UpdatedByUserId { get; set; }

    DateTimeOffset CreatedAtUtc { get; set; }

    DateTimeOffset UpdatedAtUtc { get; set; }
}
