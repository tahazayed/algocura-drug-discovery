using Algocura.LiteratureIntelligence.Api.Contracts;
using Algocura.LiteratureIntelligence.Api.Controllers;
using Algocura.LiteratureIntelligence.Api.Domain;
using Algocura.LiteratureIntelligence.Api.Events;
using Algocura.LiteratureIntelligence.Api.Messaging.Publishers;
using Algocura.LiteratureIntelligence.Api.Services;
using Algocura.LiteratureIntelligence.Api.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Algocura.LiteratureIntelligence.Api.Tests.Controllers;

public sealed class PublicationsControllerTests
{
    [Fact]
    public async Task Create_ValidRequest_CreatesPendingPublication()
    {
        await using var database = TestDatabase.Create();
        using var timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var publisher = Substitute.For<IEventPublisher>();

        var controller = new PublicationsController(
            database.Context,
            new RuleBasedLiteratureAnalysisService(),
            publisher,
            NullLogger<PublicationsController>.Instance);

        var action = await controller.Create(
            new CreatePublicationRequest(
                "LRRK2 mutation in Parkinson disease",
                "LRRK2 is associated with Parkinson disease.",
                "Example Journal",
                "PMID-1",
                "https://example.org/1",
                new DateOnly(2026, 1, 1),
                ["Scientist One"]),
            timeout.Token);

        var created = action.Result.Should()
            .BeOfType<CreatedAtActionResult>()
            .Subject;

        var response = created.Value.Should()
            .BeOfType<PublicationResponse>()
            .Subject;

        response.Status.Should()
            .Be(PublicationStatus.PendingAnalysis);

        await publisher.Received(1).PublishAsync(
            Arg.Any<PublicationRegistered>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Analyze_ExistingPublication_CreatesEvidence()
    {
        await using var database = TestDatabase.Create();
        using var timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var publication = new Publication
        {
            Id = Guid.NewGuid(),
            Title = "LRRK2 mutation in Parkinson disease",
            Abstract =
                "LRRK2 mutation is associated with Parkinson disease.",
            Status = PublicationStatus.PendingAnalysis,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        database.Context.Publications.Add(publication);
        await database.Context.SaveChangesAsync(timeout.Token);
        database.Context.ChangeTracker.Clear();

        var publisher = Substitute.For<IEventPublisher>();

        var controller = new PublicationsController(
            database.Context,
            new RuleBasedLiteratureAnalysisService(),
            publisher,
            NullLogger<PublicationsController>.Instance);

        var action = await controller.Analyze(
            publication.Id,
            new AnalyzePublicationRequest(
                "Parkinson disease",
                ["LRRK2"]),
            timeout.Token);

        var ok = action.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = ok.Value.Should()
            .BeOfType<PublicationResponse>()
            .Subject;

        response.Status.Should()
            .Be(PublicationStatus.Analyzed);

        response.EvidenceStatements.Should().ContainSingle();

        await publisher.Received(1).PublishAsync(
            Arg.Any<LiteratureEvidenceDiscovered>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}
