using Algocura.Platform.Messaging.Publishing;
using Algocura.Search.Api.Controllers;
using Algocura.Search.Api.Domain;
using Algocura.Search.Api.Events;
using Algocura.Search.Api.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Algocura.Search.Api.Tests.Controllers;

public sealed class SearchSynchronizationControllerTests
{
    [Fact]
    public async Task PublishIndexSnapshot_PublishesCounts()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var now = DateTimeOffset.UtcNow;

        database.Context.SearchDocuments.Add(
            new SearchDocument
            {
                Id = Guid.NewGuid(),
                SourceEntityId = Guid.NewGuid(),
                SourceEventType = "AssaySnapshot",
                SourceService = "Test",
                DocumentType = SearchDocumentType.Assay,
                Title = "Assay",
                Status = SearchDocumentStatus.Active,
                SourceUpdatedAtUtc = now,
                IndexedAtUtc = now,
                UpdatedAtUtc = now
            });

        await database.Context.SaveChangesAsync(
            timeout.Token);

        var publisher =
            Substitute.For<IEventPublisher>();

        var controller =
            new SearchSynchronizationController(
                database.Context,
                publisher,
                NullLogger<
                    SearchSynchronizationController>.Instance);

        var action =
            await controller.PublishIndexSnapshot(
                timeout.Token);

        var ok =
            action.Result.Should()
                .BeOfType<OkObjectResult>()
                .Subject;

        var response =
            ok.Value.Should()
                .BeOfType<SearchSynchronizationResponse>()
                .Subject;

        response.ActiveDocumentCount.Should().Be(1);

        await publisher.Received(1).PublishAsync(
            Arg.Any<SearchIndexSnapshot>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}

