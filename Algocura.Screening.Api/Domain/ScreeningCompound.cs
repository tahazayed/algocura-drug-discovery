using Algocura.Platform.Auditing.Abstractions;

namespace Algocura.Screening.Api.Domain;

public sealed class ScreeningCompound
    : IAuditableEntity,
      IProjectionAuditMetadata
{
    public Guid CompoundId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string CanonicalSmiles { get; set; } = string.Empty;

    public string? InchiKey { get; set; }

    public DateTimeOffset RegisteredAtUtc { get; set; }

    public Guid? SourceCreatedByUserId { get; set; }

    public Guid? SourceUpdatedByUserId { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
