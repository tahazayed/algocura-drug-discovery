namespace Algocura.Platform.Auditing.Abstractions;

/// <summary>
/// Defines explicit completion metadata.
/// </summary>
public interface ICompletedEntity
{
    Guid? CompletedByUserId { get; set; }

    DateTimeOffset? CompletedAtUtc { get; set; }
}

