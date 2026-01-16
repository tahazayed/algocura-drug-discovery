using Algocura.Search.Api.Contracts;
using Algocura.Search.Api.Domain;
using Algocura.Search.Api.Services;
using Algocura.Search.Api.Tests.Support;
using FluentAssertions;

namespace Algocura.Search.Api.Tests.Services;

public sealed class SearchServiceTests
{
    [Fact]
    public async Task Search_ByCompoundName_ReturnsMatch()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        database.Context.SearchDocuments.AddRange(
            CreateDocument("Aspirin", 0.90m),
            CreateDocument("Caffeine", 0.70m));

        await database.Context.SaveChangesAsync(
            timeout.Token);

        var sut =
            new SearchService(database.Context);

        var response =
            await sut.SearchAsync(
                new SearchRequest(
                    null,
                    SearchDocumentType.Compound,
                    null,
                    null,
                    "Aspirin",
                    null,
                    null,
                    null,
                    null),
                timeout.Token);

        response.Total.Should().Be(1);
        response.Items.Should().ContainSingle();
        response.Items.Single().CompoundName.Should()
            .Be("Aspirin");
    }

    private static SearchDocument CreateDocument(
        string name,
        decimal score)
    {
        var now = DateTimeOffset.UtcNow;

        return new SearchDocument
        {
            Id = Guid.NewGuid(),
            SourceEntityId = Guid.NewGuid(),
            SourceEventType = "CompoundCatalogSnapshot",
            SourceService = "Test",
            DocumentType = SearchDocumentType.Compound,
            Title = name,
            CompoundName = name,
            Score = score,
            Status = SearchDocumentStatus.Active,
            SourceUpdatedAtUtc = now,
            IndexedAtUtc = now,
            UpdatedAtUtc = now
        };
    }
}

