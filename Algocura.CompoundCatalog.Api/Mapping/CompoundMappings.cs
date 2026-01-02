using Algocura.CompoundCatalog.Api.Contracts;
using Algocura.CompoundCatalog.Api.Domain;

namespace Algocura.CompoundCatalog.Api.Mapping;

public static class CompoundMappings
{
    public static CompoundResponse ToResponse(this Compound compound)
    {
        return new CompoundResponse(
            compound.Id,
            compound.Name,
            compound.CanonicalSmiles,
            compound.InchiKey,
            compound.MolecularWeight,
            compound.MolecularFormula,
            compound.Source,
            compound.Description,
            compound.Status,
            compound.CreatedAtUtc,
            compound.UpdatedAtUtc);
    }
}