using Algocura.Platform.Auditing.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Algocura.Audit.Api.Domain;

/// <summary>
/// Immutable logical representation of an entity audit event received
/// from a platform service. Audit API persistence metadata is kept
/// separately from the actor metadata contained in the event.
/// </summary>
public sealed class AuditRecord : IAuditableEntity
{
    public Guid EventId { get; set; }

    public DateTimeOffset OccurredAtUtc { get; set; }

    [MaxLength(200)]
    public required string ServiceName { get; set; }

    [MaxLength(300)]
    public required string EntityType { get; set; }

    [MaxLength(200)]
    public required string EntityId { get; set; }

    [MaxLength(50)]
    public required string Action { get; set; }

    public Guid? UserId { get; set; }

    [MaxLength(300)]
    public string? Subject { get; set; }

    [MaxLength(300)]
    public string? UserDisplayName { get; set; }

    [MaxLength(320)]
    public string? UserEmail { get; set; }

    [MaxLength(50)]
    public required string ActorType { get; set; }

    public string? PreviousValuesJson { get; set; }

    public string? CurrentValuesJson { get; set; }

    public string? ChangedPropertiesJson { get; set; }

    [MaxLength(200)]
    public string? CorrelationId { get; set; }

    [MaxLength(100)]
    public string? WorkflowId { get; set; }

    [MaxLength(200)]
    public string? TraceId { get; set; }

    [MaxLength(100)]
    public string? IpAddress { get; set; }

    [MaxLength(1000)]
    public string? UserAgent { get; set; }

    [MaxLength(100)]
    public required string EventType { get; set; }

    [MaxLength(20)]
    public required string EventVersion { get; set; }

    public DateTimeOffset ReceivedAtUtc { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
