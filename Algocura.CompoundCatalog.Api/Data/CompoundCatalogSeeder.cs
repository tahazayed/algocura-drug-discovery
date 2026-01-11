using Algocura.CompoundCatalog.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.CompoundCatalog.Api.Data;

public static class CompoundCatalogSeeder
{
    public static async Task<IReadOnlyCollection<Compound>> SeedAsync(
        CompoundCatalogDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset timestamp =
            new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var compounds = new[]
        {
            new Compound
            {
                Id = Guid.Parse(
                    "7f06dc5b-379f-4589-9384-4f09c55f97c3"),
                Name = "Aspirin",
                CanonicalSmiles =
                    "CC(=O)OC1=CC=CC=C1C(=O)O",
                InchiKey =
                    "BSYNRYMUTXBXSQ-UHFFFAOYSA-N",
                MolecularWeight = 180.16m,
                MolecularFormula = "C9H8O4",
                Source = "Prototype seed data",
                Description =
                    "Reference compound used to demonstrate catalog operations.",
                Status = CompoundStatus.Registered,
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            },
            new Compound
            {
                Id = Guid.Parse(
                    "d2b92d72-5ce1-4659-aa35-9728dab134c6"),
                Name = "Caffeine",
                CanonicalSmiles =
                    "CN1C=NC2=C1C(=O)N(C(=O)N2C)C",
                InchiKey =
                    "RYYVLZVUVIJVGH-UHFFFAOYSA-N",
                MolecularWeight = 194.19m,
                MolecularFormula = "C8H10N4O2",
                Source = "Prototype seed data",
                Description =
                    "Reference compound used to demonstrate search operations.",
                Status = CompoundStatus.UnderEvaluation,
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            },
            new Compound
            {
                Id = Guid.Parse(
                    "c0323692-4f01-4d97-87e9-a4720b3773bb"),
                Name = "Acetaminophen",
                CanonicalSmiles =
                    "CC(=O)NC1=CC=C(C=C1)O",
                InchiKey =
                    "RZVAJINKPMORJF-UHFFFAOYSA-N",
                MolecularWeight = 151.16m,
                MolecularFormula = "C8H9NO2",
                Source = "Prototype seed data",
                Description =
                    "Reference analgesic compound.",
                Status = CompoundStatus.Registered,
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            },
            new Compound
            {
                Id = Guid.Parse(
                    "29dcde02-2703-42ee-ab50-33cf497058a5"),
                Name = "Ibuprofen",
                CanonicalSmiles =
                    "CC(C)CC1=CC=C(C=C1)C(C)C(=O)O",
                InchiKey =
                    "HEFNNWSXXWATRW-UHFFFAOYSA-N",
                MolecularWeight = 206.28m,
                MolecularFormula = "C13H18O2",
                Source = "Prototype seed data",
                Description =
                    "Reference nonsteroidal anti-inflammatory compound.",
                Status = CompoundStatus.Registered,
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            },
            new Compound
            {
                Id = Guid.Parse(
                    "ad0df37e-aab8-4cd5-830b-bb5f03031143"),
                Name = "Naproxen",
                CanonicalSmiles =
                    "COC1=CC2=C(C=C1)C=C(C=C2)[C@@H](C)C(=O)O",
                InchiKey =
                    "CMWTZPSULFXXJA-VIFPVBQESA-N",
                MolecularWeight = 230.26m,
                MolecularFormula = "C14H14O3",
                Source = "Prototype seed data",
                Description =
                    "Reference anti-inflammatory compound.",
                Status = CompoundStatus.UnderEvaluation,
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            },
            new Compound
            {
                Id = Guid.Parse(
                    "4dc2d1c6-d90d-4f57-ae62-ef03a727cc23"),
                Name = "Metformin",
                CanonicalSmiles =
                    "CN(C)C(=N)NC(=N)N",
                InchiKey =
                    "XZWYZXLIPXDOLR-UHFFFAOYSA-N",
                MolecularWeight = 129.16m,
                MolecularFormula = "C4H11N5",
                Source = "Prototype seed data",
                Description =
                    "Reference biguanide compound.",
                Status = CompoundStatus.Selected,
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            },
            new Compound
            {
                Id = Guid.Parse(
                    "68643fc3-4368-41fd-9360-cd6847bdc841"),
                Name = "Dopamine",
                CanonicalSmiles =
                    "C1=CC(=C(C=C1CCN)O)O",
                InchiKey =
                    "VYFYYTLLBUKUHU-UHFFFAOYSA-N",
                MolecularWeight = 153.18m,
                MolecularFormula = "C8H11NO2",
                Source = "Prototype seed data",
                Description =
                    "Reference catecholamine neurotransmitter.",
                Status = CompoundStatus.UnderEvaluation,
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            },
            new Compound
            {
                Id = Guid.Parse(
                    "77d22da0-4ae0-4ef0-b65d-bc8591a9ad57"),
                Name = "Lidocaine",
                CanonicalSmiles =
                    "CCN(CC)C(=O)C1=C(NC2=CC=CC=C2)C=CC=C1",
                InchiKey =
                    "NNJVILVZKWQKPM-UHFFFAOYSA-N",
                MolecularWeight = 234.34m,
                MolecularFormula = "C14H22N2O",
                Source = "Prototype seed data",
                Description =
                    "Reference local anesthetic compound.",
                Status = CompoundStatus.Registered,
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            },
            new Compound
            {
                Id = Guid.Parse(
                    "c44d52df-eb67-4755-a774-dae08d6ac76c"),
                Name = "Warfarin",
                CanonicalSmiles =
                    "CC(C1=CC=CC=C1)C(C2=CC=CC=C2O)=O",
                InchiKey =
                    "PJVWKTKQMONHTI-UHFFFAOYSA-N",
                MolecularWeight = 308.33m,
                MolecularFormula = "C19H16O4",
                Source = "Prototype seed data",
                Description =
                    "Reference anticoagulant compound.",
                Status = CompoundStatus.Registered,
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            },
            new Compound
            {
                Id = Guid.Parse(
                    "2e63343e-6816-40ea-8c3f-2e80351a8d65"),
                Name = "Salicylic Acid",
                CanonicalSmiles =
                    "O=C(O)C1=CC=CC=C1O",
                InchiKey =
                    "YGSDEFSMJLZEOE-UHFFFAOYSA-N",
                MolecularWeight = 138.12m,
                MolecularFormula = "C7H6O3",
                Source = "Prototype seed data",
                Description =
                    "Reference aromatic hydroxy acid.",
                Status = CompoundStatus.Draft,
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            },
            new Compound
            {
                Id = Guid.Parse(
                    "b89eed92-03bb-4a0f-b860-e827816187e8"),
                Name = "Benzoic Acid",
                CanonicalSmiles =
                    "O=C(O)C1=CC=CC=C1",
                InchiKey =
                    "WPYMKLBDIGXBTP-UHFFFAOYSA-N",
                MolecularWeight = 122.12m,
                MolecularFormula = "C7H6O2",
                Source = "Prototype seed data",
                Description =
                    "Reference aromatic carboxylic acid.",
                Status = CompoundStatus.Archived,
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            },
            new Compound
            {
                Id = Guid.Parse(
                    "20ad34b2-f7ab-443c-b321-c2e06a6d10f5"),
                Name = "Ethanol",
                CanonicalSmiles = "CCO",
                InchiKey =
                    "LFQSCWFLJHTTHZ-UHFFFAOYSA-N",
                MolecularWeight = 46.07m,
                MolecularFormula = "C2H6O",
                Source = "Prototype seed data",
                Description =
                    "Reference small-molecule solvent.",
                Status = CompoundStatus.Rejected,
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            }
        };

        var seedIds = compounds
            .Select(compound => compound.Id)
            .ToArray();

        var existingIds = await dbContext.Compounds
            .Where(compound =>
                seedIds.Contains(compound.Id))
            .Select(compound => compound.Id)
            .ToListAsync(cancellationToken);

        var existingIdSet = existingIds.ToHashSet();

        var missingCompounds = compounds
            .Where(compound =>
                !existingIdSet.Contains(compound.Id))
            .ToArray();

        if (missingCompounds.Length > 0)
        {
            await dbContext.Compounds.AddRangeAsync(
                missingCompounds,
                cancellationToken);

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }

        // Return every configured seed compound so the initializer
        // can publish a complete synchronization snapshot.
        return compounds;
    }
}
