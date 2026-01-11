namespace Algocura.Screening.Api.Domain;

public sealed class ScreeningCompound
{
    public Guid CompoundId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CanonicalSmiles { get; set; } = string.Empty;
    public string? InchiKey { get; set; }
    public DateTimeOffset RegisteredAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
