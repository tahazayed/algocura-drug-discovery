namespace Algocura.Screening.Api.Domain;

public sealed class ScreeningResult
{
    public Guid Id { get; set; }
    public Guid ScreeningJobId { get; set; }
    public Guid CompoundId { get; set; }
    public decimal Score { get; set; }
    public int Rank { get; set; }
    public bool Selected { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public ScreeningJob ScreeningJob { get; set; } = null!;
}
