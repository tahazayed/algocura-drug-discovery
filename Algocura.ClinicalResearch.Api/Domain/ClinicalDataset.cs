using Algocura.Platform.Auditing.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Algocura.ClinicalResearch.Api.Domain;

public sealed class ClinicalDataset : IAuditableEntity
{
    public Guid Id { get; set; }

    [MaxLength(160)]
    public required string Name { get; set; }

    [MaxLength(120)]
    public required string SourceName { get; set; }

    [MaxLength(1000)]
    public string? SourceUri { get; set; }

    [MaxLength(100)]
    public string? StudyCode { get; set; }

    [MaxLength(160)]
    public string? DiseaseArea { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public ClinicalDatasetStatus Status { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }

    public ICollection<ResearchSubject> Subjects { get; set; } = [];
}
