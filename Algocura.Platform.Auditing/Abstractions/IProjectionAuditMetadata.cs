namespace Algocura.Platform.Auditing.Abstractions;

/// <summary>
/// Defines source-user metadata for read models and event-driven projections.
/// </summary>
public interface IProjectionAuditMetadata
{
    Guid? SourceCreatedByUserId { get; set; }

    Guid? SourceUpdatedByUserId { get; set; }
}

