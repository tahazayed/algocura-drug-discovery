using Algocura.ChemicalDesign.Api.Domain;
using Algocura.ChemicalDesign.Api.Services;
using FluentAssertions;

namespace Algocura.ChemicalDesign.Api.Tests.Services;

public sealed class StructureSearchEngineTests
{
    [Fact]
    public void Search_ExactMatch_ReturnsMatchingEntry()
    {
        var query = new QueryStructure
        {
            Id = Guid.NewGuid(),
            DesignProjectId = Guid.NewGuid(),
            Name = "Aspirin",
            Structure = "CC(=O)OC1=CC=CC=C1C(=O)O",
            StructureFormat = "SMILES",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        var library = new[]
        {
            new ChemicalLibraryEntry
            {
                Id = Guid.NewGuid(),
                Name = "Aspirin",
                CanonicalSmiles = "CC(=O)OC1=CC=CC=C1C(=O)O",
                MolecularWeight = 180.16m,
                SourceReference = "Test"
            },
            new ChemicalLibraryEntry
            {
                Id = Guid.NewGuid(),
                Name = "Caffeine",
                CanonicalSmiles = "CN1C=NC2=C1C(=O)N(C(=O)N2C)C",
                MolecularWeight = 194.19m,
                SourceReference = "Test"
            }
        };

        var sut = new DeterministicStructureSearchEngine();

        var results = sut.Search(
            query,
            StructureSearchType.Exact,
            1m,
            10,
            library);

        results.Should().ContainSingle();
        results.Single().Entry.Name.Should().Be("Aspirin");
        results.Single().MatchScore.Should().Be(1m);
    }

    [Fact]
    public void Search_InvalidThreshold_Throws()
    {
        var sut = new DeterministicStructureSearchEngine();

        var action = () => sut.Search(
            new QueryStructure
            {
                Id = Guid.NewGuid(),
                DesignProjectId = Guid.NewGuid(),
                Name = "Query",
                Structure = "CCO",
                StructureFormat = "SMILES",
                CreatedAtUtc = DateTimeOffset.UtcNow
            },
            StructureSearchType.Similarity,
            1.2m,
            10,
            []);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}
