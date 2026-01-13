using Algocura.TargetDiscovery.Api.Contracts;
using Algocura.TargetDiscovery.Api.Controllers;
using Algocura.TargetDiscovery.Api.Domain;
using Algocura.TargetDiscovery.Api.Events;
using Algocura.TargetDiscovery.Api.Messaging.Publishers;
using Algocura.TargetDiscovery.Api.Services;
using Algocura.TargetDiscovery.Api.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Algocura.TargetDiscovery.Api.Tests.Controllers;

public sealed class TargetAssessmentsControllerTests
{
    [Fact]
    public async Task Create_KnownHypothesis_CreatesDraftAssessment()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var hypothesis =
            CreateHypothesis();

        database.Context.TargetHypotheses.Add(
            hypothesis);

        await database.Context.SaveChangesAsync(
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        var publisher =
            Substitute.For<IEventPublisher>();

        var controller =
            CreateController(
                database.Context,
                publisher);

        var action = await controller.Create(
            new CreateTargetAssessmentRequest(
                hypothesis.TargetHypothesisId,
                "LRRK2 target assessment",
                "Evaluate LRRK2 inhibition.",
                0.90m,
                0.85m,
                0.80m,
                0.20m,
                0.80m,
                "Initial review"),
            timeout.Token);

        var created = action.Result.Should()
            .BeOfType<CreatedAtActionResult>()
            .Subject;

        var response = created.Value.Should()
            .BeOfType<TargetAssessmentResponse>()
            .Subject;

        response.Status.Should()
            .Be(TargetAssessmentStatus.Draft);

        response.LiteratureEvidenceScore.Should()
            .Be(hypothesis.LiteratureEvidenceScore);

        await publisher.Received(1).PublishAsync(
            Arg.Any<TargetAssessmentCreated>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Complete_DraftAssessment_CalculatesScore()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var hypothesis =
            CreateHypothesis();

        var assessment =
            CreateAssessment(hypothesis);

        database.Context.TargetHypotheses.Add(hypothesis);
        database.Context.TargetAssessments.Add(assessment);

        await database.Context.SaveChangesAsync(
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        var publisher =
            Substitute.For<IEventPublisher>();

        var controller =
            CreateController(
                database.Context,
                publisher);

        var action = await controller.Complete(
            assessment.Id,
            timeout.Token);

        var ok = action.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = ok.Value.Should()
            .BeOfType<TargetAssessmentResponse>()
            .Subject;

        response.Status.Should()
            .Be(TargetAssessmentStatus.Completed);

        response.OverallScore.Should()
            .BeGreaterThan(0m);

        await publisher.Received(1).PublishAsync(
            Arg.Any<TargetAssessmentCompleted>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Select_CompletedAssessment_PublishesTargetSelected()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var hypothesis =
            CreateHypothesis();

        var assessment =
            CreateAssessment(hypothesis);

        assessment.Status =
            TargetAssessmentStatus.Completed;

        assessment.OverallScore = 0.84m;

        assessment.ConfidenceLevel =
            TargetConfidenceLevel.High;

        assessment.CompletedAtUtc =
            DateTimeOffset.UtcNow;

        database.Context.TargetHypotheses.Add(hypothesis);
        database.Context.TargetAssessments.Add(assessment);

        await database.Context.SaveChangesAsync(
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        var publisher =
            Substitute.For<IEventPublisher>();

        var controller =
            CreateController(
                database.Context,
                publisher);

        var action = await controller.Select(
            assessment.Id,
            timeout.Token);

        var ok = action.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = ok.Value.Should()
            .BeOfType<TargetAssessmentResponse>()
            .Subject;

        response.Status.Should()
            .Be(TargetAssessmentStatus.Selected);

        await publisher.Received(1).PublishAsync(
            Arg.Any<TargetSelected>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    private static TargetAssessmentsController CreateController(
        Data.TargetDiscoveryDbContext dbContext,
        IEventPublisher publisher)
    {
        return new TargetAssessmentsController(
            dbContext,
            new DeterministicTargetAssessmentScoringEngine(),
            publisher,
            NullLogger<TargetAssessmentsController>.Instance);
    }

    private static TargetHypothesisProjection CreateHypothesis()
    {
        return new TargetHypothesisProjection
        {
            TargetHypothesisId = Guid.NewGuid(),
            DiseaseName = "Parkinson disease",
            GeneSymbol = "LRRK2",
            ProteinTarget =
                "Leucine-rich repeat kinase 2",
            DesiredModulation = "Inhibit",
            Rationale =
                "Literature-supported hypothesis.",
            LiteratureEvidenceScore = 0.82m,
            Status =
                TargetHypothesisProjectionStatus.Published,
            SourceUpdatedAtUtc =
                DateTimeOffset.UtcNow,
            SynchronizedAtUtc =
                DateTimeOffset.UtcNow
        };
    }

    private static TargetAssessment CreateAssessment(
        TargetHypothesisProjection hypothesis)
    {
        return new TargetAssessment
        {
            Id = Guid.NewGuid(),
            TargetHypothesisId =
                hypothesis.TargetHypothesisId,
            Name = "LRRK2 target assessment",
            LiteratureEvidenceScore =
                hypothesis.LiteratureEvidenceScore,
            GeneticEvidenceScore = 0.90m,
            BiologicalPlausibilityScore = 0.85m,
            DruggabilityScore = 0.80m,
            SafetyRiskScore = 0.20m,
            TranslationalPotentialScore = 0.80m,
            Status = TargetAssessmentStatus.Draft,
            ConfidenceLevel =
                TargetConfidenceLevel.Low,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
    }
}
