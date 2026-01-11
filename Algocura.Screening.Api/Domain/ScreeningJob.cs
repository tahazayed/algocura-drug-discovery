namespace Algocura.Screening.Api.Domain;

public sealed class ScreeningJob
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ScreeningMethod { get; set; } = string.Empty;
    public ScreeningStatus Status { get; set; } = ScreeningStatus.Draft;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public DateTimeOffset? StartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public string? FailureReason { get; set; }
    public List<ScreeningCandidate> Candidates { get; set; } = [];
    public List<ScreeningResult> Results { get; set; } = [];
}
