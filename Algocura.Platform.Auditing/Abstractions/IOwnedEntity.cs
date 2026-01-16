namespace Algocura.Platform.Auditing.Abstractions;

/// <summary>
/// Defines explicit ownership metadata for entities that belong to a user.
/// </summary>
public interface IOwnedEntity
{
    Guid? OwnerUserId { get; set; }
}

