using Algocura.CompoundCatalog.Api.Domain;

namespace Algocura.CompoundCatalog.Api.Data;

public static class CompoundSeedData
{
    public static IReadOnlyCollection<Compound> GetCompounds()
    {
        var createdAt = new DateTime(
            2026,
            1,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        return
        [
            Create(
                id: "9b32758d-728d-46a9-954f-f585d7b5299a",
                name: "Aspirin",
                formula: "C9H8O4",
                canonicalSmiles: "CC(=O)OC1=CC=CC=C1C(=O)O",
                inChIKey: "BSYNRYMUTXBXSQ-UHFFFAOYSA-N",
                createdAt),

            Create(
                id: "d89c37f8-64a0-4fbb-8675-64b36fb96898",
                name: "Caffeine",
                formula: "C8H10N4O2",
                canonicalSmiles: "CN1C=NC2=C1C(=O)N(C(=O)N2C)C",
                inChIKey: "RYYVLZVUVIJVGH-UHFFFAOYSA-N",
                createdAt),

            Create(
                id: "1dcf475f-194f-4e62-8a35-f1a85e593d6a",
                name: "Acetaminophen",
                formula: "C8H9NO2",
                canonicalSmiles: "CC(=O)NC1=CC=C(C=C1)O",
                inChIKey: "RZVAJINKPMORJF-UHFFFAOYSA-N",
                createdAt),

            Create(
                id: "92953251-6d37-4135-be86-c321c59c9c90",
                name: "Ibuprofen",
                formula: "C13H18O2",
                canonicalSmiles: "CC(C)CC1=CC=C(C=C1)C(C)C(=O)O",
                inChIKey: "HEFNNWSXXWATRW-UHFFFAOYSA-N",
                createdAt),

            Create(
                id: "63892df8-da9e-4309-9778-2c30cc972150",
                name: "Naproxen",
                formula: "C14H14O3",
                canonicalSmiles: "COC1=CC2=C(C=C1)C=C(C=C2)[C@@H](C)C(=O)O",
                inChIKey: "CMWTZPSULFXXJA-VIFPVBQESA-N",
                createdAt),

            Create(
                id: "39081cb2-6e69-41e8-8854-70df8edbd35d",
                name: "Metformin",
                formula: "C4H11N5",
                canonicalSmiles: "CN(C)C(=N)NC(=N)N",
                inChIKey: "XZWYZXLIPXDOLR-UHFFFAOYSA-N",
                createdAt),

            Create(
                id: "843ec1bf-2176-4399-a721-beb8c0a7217d",
                name: "Lidocaine",
                formula: "C14H22N2O",
                canonicalSmiles: "CCN(CC)C(=O)C1=C(NC2=CC=CC=C2)C=CC=C1",
                inChIKey: "NNJVILVZKWQKPM-UHFFFAOYSA-N",
                createdAt),

            Create(
                id: "18c6516f-34a8-47a8-9ca3-dd7cb27879e0",
                name: "Dopamine",
                formula: "C8H11NO2",
                canonicalSmiles: "C1=CC(=C(C=C1CCN)O)O",
                inChIKey: "VYFYYTLLBUKUHU-UHFFFAOYSA-N",
                createdAt),

            Create(
                id: "ddcf72d6-e30d-420d-83b9-8a6eddb92b99",
                name: "Warfarin",
                formula: "C19H16O4",
                canonicalSmiles: "CC(C1=CC=CC=C1)C(C2=CC=CC=C2O)=O",
                inChIKey: "PJVWKTKQMONHTI-UHFFFAOYSA-N",
                createdAt),

            Create(
                id: "21d9fc46-8414-430a-95d7-0611440d1fb9",
                name: "Ethanol",
                formula: "C2H6O",
                canonicalSmiles: "CCO",
                inChIKey: "LFQSCWFLJHTTHZ-UHFFFAOYSA-N",
                createdAt),

            Create(
                id: "542dd71a-5af8-4fc7-b47d-f0abb8093dae",
                name: "Benzoic acid",
                formula: "C7H6O2",
                canonicalSmiles: "O=C(O)C1=CC=CC=C1",
                inChIKey: "WPYMKLBDIGXBTP-UHFFFAOYSA-N",
                createdAt),

            Create(
                id: "ecb15ddd-8170-4093-b99c-18139bd094ab",
                name: "Salicylic acid",
                formula: "C7H6O3",
                canonicalSmiles: "O=C(O)C1=CC=CC=C1O",
                inChIKey: "YGSDEFSMJLZEOE-UHFFFAOYSA-N",
                createdAt)
        ];
    }

    private static Compound Create(
        string id,
        string name,
        string formula,
        string canonicalSmiles,
        string inChIKey,
        DateTime createdAt)
    {
        return new Compound
        {
            Id = Guid.Parse(id),
            Name = name,
            MolecularFormula = formula,
            CanonicalSmiles = canonicalSmiles,
            InchiKey = inChIKey,
            Status = CompoundStatus.Registered,
            CreatedAtUtc = createdAt,
            UpdatedAtUtc = createdAt,
            CreatedByUserId = SystemUsers.SystemUserId,
            UpdatedByUserId = SystemUsers.SystemUserId
        };
    }
}