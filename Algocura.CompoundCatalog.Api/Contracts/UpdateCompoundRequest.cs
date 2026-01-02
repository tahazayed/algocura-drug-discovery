using Algocura.CompoundCatalog.Api.Domain;
using System.ComponentModel.DataAnnotations;

namespace Algocura.CompoundCatalog.Api.Contracts;

public sealed record UpdateCompoundRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string Name { get; init; }

    [Required]
    [StringLength(250, MinimumLength = 1)]
    public required string CanonicalSmiles { get; init; }

    [StringLength(27, MinimumLength = 27)]
    public string? InchiKey { get; init; }

    [Range(0.001, 100000)]
    public decimal MolecularWeight { get; init; }

    [StringLength(100)]
    public string? MolecularFormula { get; init; }

    [StringLength(100)]
    public string? Source { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }

    public CompoundStatus Status { get; init; }
}