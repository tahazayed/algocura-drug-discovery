namespace Algocura.Screening.Api.Domain;

public sealed class ScreeningCandidate
{
    public Guid Id { get; set; }
    public Guid ScreeningJobId { get; set; }
    public Guid CompoundId { get; set; }
    public string CompoundName { get; set; } = string.Empty;
    public string CanonicalSmiles { get; set; } = string.Empty;
    public DateTimeOffset AddedAtUtc { get; set; }
    public ScreeningJob ScreeningJob { get; set; } = null!;
}
