using Algocura.ChemicalDesign.Api.Contracts;
using Algocura.ChemicalDesign.Api.Controllers;
using Algocura.ChemicalDesign.Api.Domain;
using Algocura.ChemicalDesign.Api.Events;
using Algocura.ChemicalDesign.Api.Messaging.Publishers;
using Algocura.ChemicalDesign.Api.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Algocura.ChemicalDesign.Api.Tests.Controllers;

public sealed class DesignProjectsControllerTests
{
    [Fact]
    public async Task Create_KnownTarget_CreatesDraftProject()
    {
        await using var database = TestDatabase.Create();
        using var timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var target = new SelectedTargetProjection
        {
            TargetAssessmentId = Guid.NewGuid(),
            TargetHypothesisId = Guid.NewGuid(),
            DiseaseName = "Parkinson disease",
            GeneSymbol = "LRRK2",
            DesiredModulation = "Inhibit",
            Rationale = "Validated target",
            LiteratureEvidenceScore = 0.82m,
            TargetAssessmentScore = 0.84m,
            ConfidenceLevel = "High",
            SourceUpdatedAtUtc = DateTimeOffset.UtcNow,
            SynchronizedAtUtc = DateTimeOffset.UtcNow
        };

        database.Context.SelectedTargets.Add(target);
        await database.Context.SaveChangesAsync(timeout.Token);
        database.Context.ChangeTracker.Clear();

        var publisher = Substitute.For<IEventPublisher>();

        var controller = new DesignProjectsController(
            database.Context,
            publisher,
            NullLogger<DesignProjectsController>.Instance);

        var action = await controller.Create(
            new CreateDesignProjectRequest(
                target.TargetAssessmentId,
                "LRRK2 inhibitor design",
                "Identify candidate structures."),
            timeout.Token);

        var created = action.Result.Should()
            .BeOfType<CreatedAtActionResult>().Subject;

        var response = created.Value.Should()
            .BeOfType<DesignProjectResponse>().Subject;

        response.Status.Should().Be(DesignProjectStatus.Draft);

        await publisher.Received(1).PublishAsync(
            Arg.Any<ChemicalDesignProjectCreated>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}
