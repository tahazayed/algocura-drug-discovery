using Algocura.CompoundCatalog.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.CompoundCatalog.Api.Data;

public static class CompoundCatalogSeeder
{
    public static async Task SeedAsync(
        CompoundCatalogDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Compounds.AnyAsync(cancellationToken))
        {
            return;
        }

        var timestamp = DateTimeOffset.UtcNow;

        var compounds = new[]
        {
            new Compound
            {
                Id = Guid.Parse("7f06dc5b-379f-4589-9384-4f09c55f97c3"),
                Name = "Aspirin",
                CanonicalSmiles = "CC(=O)OC1=CC=CC=C1C(=O)O",
                InchiKey = "BSYNRYMUTXBXSQ-UHFFFAOYSA-N",
                MolecularWeight = 180.16m,
                MolecularFormula = "C9H8O4",
                Source = "Prototype seed data",
                Description = "Reference compound used to demonstrate catalog operations.",
                Status = CompoundStatus.Registered,
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            },
            new Compound
            {
                Id = Guid.Parse("d2b92d72-5ce1-4659-aa35-9728dab134c6"),
                Name = "Caffeine",
                CanonicalSmiles = "CN1C=NC2=C1C(=O)N(C(=O)N2C)C",
                InchiKey = "RYYVLZVUVIJVGH-UHFFFAOYSA-N",
                MolecularWeight = 194.19m,
                MolecularFormula = "C8H10N4O2",
                Source = "Prototype seed data",
                Description = "Reference compound used to demonstrate search operations.",
                Status = CompoundStatus.UnderEvaluation,
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            }
        };

        await dbContext.Compounds.AddRangeAsync(compounds, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}