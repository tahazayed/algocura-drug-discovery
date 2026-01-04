using Algocura.CompoundCatalog.Api.Data;
using FluentAssertions;

namespace Algocura.CompoundCatalog.Api.Tests.Data;

public sealed class CompoundSeedDataTests
{
    [Fact]
    public void GetCompounds_ShouldReturnMultipleCompounds()
    {
        var compounds = CompoundSeedData.GetCompounds();

        compounds.Should().HaveCountGreaterThanOrEqualTo(10);
    }

    [Fact]
    public void GetCompounds_ShouldHaveUniqueIdentifiers()
    {
        var compounds = CompoundSeedData.GetCompounds();

        compounds
            .Select(x => x.Id)
            .Should()
            .OnlyHaveUniqueItems();

        compounds
            .Select(x => x.InchiKey)
            .Should()
            .OnlyHaveUniqueItems();
    }

    [Fact]
    public void GetCompounds_ShouldContainRequiredChemicalIdentifiers()
    {
        var compounds = CompoundSeedData.GetCompounds();

        compounds.Should().OnlyContain(x =>
            !string.IsNullOrWhiteSpace(x.Name) &&
            !string.IsNullOrWhiteSpace(x.MolecularFormula) &&
            !string.IsNullOrWhiteSpace(x.CanonicalSmiles) &&
            !string.IsNullOrWhiteSpace(x.InchiKey));
    }
}