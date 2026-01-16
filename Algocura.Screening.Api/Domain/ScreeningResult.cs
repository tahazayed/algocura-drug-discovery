using Algocura.Platform.Auditing.Abstractions;

namespace Algocura.Screening.Api.Domain;

public sealed class ScreeningResult
    : IAuditableEntity
{
    public Guid Id { get; set; }

    public Guid ScreeningJobId { get; set; }

    public Guid CompoundId { get; set; }

    public decimal Score { get; set; }

    public int Rank { get; set; }

    public bool Selected { get; set; }

    public Guid? SelectedByUserId { get; set; }

    public DateTimeOffset? SelectedAtUtc { get; set; }

    public string? Notes { get; set; }

    public ScreeningJob ScreeningJob { get; set; } = null!;

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
