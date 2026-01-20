using Algocura.Platform.Messaging.Publishing;
using Algocura.Search.Api.Domain;
using Algocura.Search.Api.Events;
using Algocura.Search.Api.Messaging.Consumers;
using Algocura.Search.Api.Services;
using Algocura.Search.Api.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Algocura.Search.Api.Tests.Messaging;

public sealed class PlatformSearchEventsConsumerTests
{
    [Fact]
    public async Task Handle_SupportedEvent_CreatesDocument()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var publisher =
            Substitute.For<IEventPublisher>();

        var compoundId = Guid.NewGuid();

        var payload =
            "{" +
            "\"compoundId\":\"" + compoundId + "\"," +
            "\"name\":\"Aspirin\"" +
            "}";

        var created =
            await PlatformSearchEventsConsumer.HandleAsync(
                database.Context,
                publisher,
                new PlatformSearchEvent(
                    Guid.NewGuid(),
                    "CompoundCatalogSnapshot",
                    "Algocura.CompoundCatalog.Api",
                    null,
                    DateTimeOffset.UtcNow,
                    payload),
                timeout.Token);

        created.Should().BeTrue();

        var document =
            await database.Context.SearchDocuments
                .SingleAsync(
                    x => x.SourceEntityId == compoundId,
                    timeout.Token);

        document.DocumentType.Should()
            .Be(SearchDocumentType.Compound);

        await publisher.Received(1).PublishAsync(
            Arg.Any<SearchDocumentIndexed>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}

