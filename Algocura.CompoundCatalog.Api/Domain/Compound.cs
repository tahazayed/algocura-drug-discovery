using Algocura.Platform.Auditing.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Algocura.CompoundCatalog.Api.Domain;

public sealed class Compound : IAuditableEntity
{
    public Guid Id { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(250)]
    public required string CanonicalSmiles { get; set; }

    [MaxLength(27)]
    public string? InchiKey { get; set; }

    [Range(0.001, 100000)]
    public decimal MolecularWeight { get; set; }

    [MaxLength(100)]
    public string? MolecularFormula { get; set; }

    [MaxLength(100)]
    public string? Source { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public CompoundStatus Status { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }
}