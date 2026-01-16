using Algocura.Platform.Auditing.Abstractions;

namespace Algocura.Screening.Api.Domain;

public sealed class ScreeningCandidate
    : IAuditableEntity
{
    public Guid Id { get; set; }

    public Guid ScreeningJobId { get; set; }

    public Guid CompoundId { get; set; }

    public string CompoundName { get; set; } = string.Empty;

    public string CanonicalSmiles { get; set; } = string.Empty;

    public DateTimeOffset AddedAtUtc { get; set; }

    public ScreeningJob ScreeningJob { get; set; } = null!;

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
