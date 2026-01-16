namespace Algocura.Platform.Auditing.Abstractions;

/// <summary>
/// Defines explicit approval metadata.
/// </summary>
public interface IApprovedEntity
{
    Guid? ApprovedByUserId { get; set; }

    DateTimeOffset? ApprovedAtUtc { get; set; }
}
