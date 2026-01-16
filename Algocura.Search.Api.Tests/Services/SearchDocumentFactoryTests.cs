using Algocura.Search.Api.Domain;
using Algocura.Search.Api.Services;
using FluentAssertions;

namespace Algocura.Search.Api.Tests.Services;

public sealed class SearchDocumentFactoryTests
{
    [Fact]
    public void Create_CompoundSnapshot_MapsDocument()
    {
        var compoundId = Guid.NewGuid();

        var payload =
            "{" +
            "\"compoundId\":\"" + compoundId + "\"," +
            "\"name\":\"Aspirin\"," +
            "\"inchiKey\":\"BSYNRYMUTXBXSQ-UHFFFAOYSA-N\"" +
            "}";

        var result =
            SearchDocumentFactory.Create(
                new PlatformSearchEvent(
                    Guid.NewGuid(),
                    "CompoundCatalogSnapshot",
                    "Algocura.CompoundCatalog.Api",
                    "workflow-1",
                    DateTimeOffset.UtcNow,
                    payload));

        result.Should().NotBeNull();
        result!.DocumentType.Should()
            .Be(SearchDocumentType.Compound);
        result.SourceEntityId.Should()
            .Be(compoundId);
        result.CompoundName.Should()
            .Be("Aspirin");
    }

    [Fact]
    public void Create_UnsupportedEvent_ReturnsNull()
    {
        var result =
            SearchDocumentFactory.Create(
                new PlatformSearchEvent(
                    Guid.NewGuid(),
                    "UnknownEvent",
                    "Unknown",
                    null,
                    DateTimeOffset.UtcNow,
                    "{}"));

        result.Should().BeNull();
    }
}

