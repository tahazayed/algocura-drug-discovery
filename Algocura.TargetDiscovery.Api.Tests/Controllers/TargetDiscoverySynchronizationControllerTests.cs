using Algocura.Platform.Messaging.Publishing;
using Algocura.TargetDiscovery.Api.Controllers;
using Algocura.TargetDiscovery.Api.Domain;
using Algocura.TargetDiscovery.Api.Events;
using Algocura.TargetDiscovery.Api.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Algocura.TargetDiscovery.Api.Tests.Controllers;

public sealed class TargetDiscoverySynchronizationControllerTests
{
    [Fact]
    public async Task SynchronizeSelectedTargets_PublishesOnlySelectedAssessments()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var selectedHypothesis =
            CreateHypothesis("LRRK2");

        var draftHypothesis =
            CreateHypothesis("SNCA");

        database.Context.TargetHypotheses.AddRange(
            selectedHypothesis,
            draftHypothesis);

        database.Context.TargetAssessments.AddRange(
            CreateAssessment(
                selectedHypothesis,
                TargetAssessmentStatus.Selected),
            CreateAssessment(
                draftHypothesis,
                TargetAssessmentStatus.Draft));

        await database.Context.SaveChangesAsync(
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        var publisher =
            Substitute.For<IEventPublisher>();

        var controller =
            new TargetDiscoverySynchronizationController(
                database.Context,
                publisher,
                NullLogger<
                    TargetDiscoverySynchronizationController>.Instance);

        var action =
            await controller.SynchronizeSelectedTargets(
                timeout.Token);

        var ok = action.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = ok.Value.Should()
            .BeOfType<
                TargetDiscoverySynchronizationResponse>()
            .Subject;

        response.TotalSelectedTargets.Should().Be(1);
        response.PublishedEvents.Should().Be(1);
        response.FailedEvents.Should().Be(0);

        await publisher.Received(1).PublishAsync(
            Arg.Any<TargetDiscoverySnapshot>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    private static TargetHypothesisProjection CreateHypothesis(
        string gene)
    {
        return new TargetHypothesisProjection
        {
            TargetHypothesisId = Guid.NewGuid(),
            DiseaseName = "Parkinson disease",
            GeneSymbol = gene,
            ProteinTarget = gene,
            DesiredModulation = "Inhibit",
            Rationale = "Test rationale",
            LiteratureEvidenceScore = 0.80m,
            Status =
                TargetHypothesisProjectionStatus.Published,
            SourceUpdatedAtUtc =
                DateTimeOffset.UtcNow,
            SynchronizedAtUtc =
                DateTimeOffset.UtcNow
        };
    }

    private static TargetAssessment CreateAssessment(
        TargetHypothesisProjection hypothesis,
        TargetAssessmentStatus status)
    {
        return new TargetAssessment
        {
            Id = Guid.NewGuid(),
            TargetHypothesisId =
                hypothesis.TargetHypothesisId,
            Name = $"{hypothesis.GeneSymbol} assessment",
            LiteratureEvidenceScore = 0.80m,
            GeneticEvidenceScore = 0.80m,
            BiologicalPlausibilityScore = 0.80m,
            DruggabilityScore = 0.80m,
            SafetyRiskScore = 0.20m,
            TranslationalPotentialScore = 0.80m,
            OverallScore = 0.80m,
            ConfidenceLevel =
                TargetConfidenceLevel.High,
            Status = status,
            CreatedAtUtc =
                DateTimeOffset.UtcNow,
            UpdatedAtUtc =
                DateTimeOffset.UtcNow,
            CompletedAtUtc =
                status == TargetAssessmentStatus.Selected
                    ? DateTimeOffset.UtcNow
                    : null,
            SelectedAtUtc =
                status == TargetAssessmentStatus.Selected
                    ? DateTimeOffset.UtcNow
                    : null
        };
    }
}
