using Algocura.LiteratureIntelligence.Api.Contracts;
using Algocura.LiteratureIntelligence.Api.Controllers;
using Algocura.LiteratureIntelligence.Api.Domain;
using Algocura.LiteratureIntelligence.Api.Events;
using Algocura.LiteratureIntelligence.Api.Tests.Support;
using Algocura.Platform.Messaging.Publishing;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Algocura.LiteratureIntelligence.Api.Tests.Controllers;

public sealed class TargetHypothesesControllerTests
{
    [Fact]
    public async Task Create_ValidEvidence_CreatesProposedHypothesis()
    {
        await using var database = TestDatabase.Create();
        using var timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var publication = new Publication
        {
            Id = Guid.NewGuid(),
            Title = "Publication",
            Abstract = "Abstract",
            Status = PublicationStatus.Analyzed,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        var evidence = new EvidenceStatement
        {
            Id = Guid.NewGuid(),
            PublicationId = publication.Id,
            DiseaseName = "Parkinson disease",
            GeneSymbol = "LRRK2",
            RelationshipType = "GeneticAssociation",
            Statement = "LRRK2 is associated with Parkinson disease.",
            Strength = EvidenceStrength.Strong,
            ConfidenceScore = 0.80m,
            HumanValidated = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        database.Context.Publications.Add(publication);
        database.Context.EvidenceStatements.Add(evidence);
        await database.Context.SaveChangesAsync(timeout.Token);
        database.Context.ChangeTracker.Clear();

        var publisher = Substitute.For<IEventPublisher>();

        var controller = new TargetHypothesesController(
            database.Context,
            publisher,
            NullLogger<TargetHypothesesController>.Instance);

        var action = await controller.Create(
            new CreateTargetHypothesisRequest(
                "Parkinson disease",
                "LRRK2",
                "Leucine-rich repeat kinase 2",
                "Inhibit",
                "Evidence supports evaluation of LRRK2 inhibition.",
                [evidence.Id]),
            timeout.Token);

        var created = action.Result.Should()
            .BeOfType<CreatedAtActionResult>()
            .Subject;

        var response = created.Value.Should()
            .BeOfType<TargetHypothesisResponse>()
            .Subject;

        response.Status.Should()
            .Be(TargetHypothesisStatus.Proposed);

        response.EvidenceScore.Should().Be(0.80m);

        await publisher.Received(1).PublishAsync(
            Arg.Any<TargetHypothesisProposed>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}
