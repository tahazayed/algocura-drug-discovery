using Algocura.ChemicalDesign.Api.Controllers;
using Algocura.ChemicalDesign.Api.Domain;
using Algocura.ChemicalDesign.Api.Events;
using Algocura.ChemicalDesign.Api.Messaging.Publishers;
using Algocura.ChemicalDesign.Api.Services;
using Algocura.ChemicalDesign.Api.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Algocura.ChemicalDesign.Api.Tests.Controllers;

public sealed class StructureSearchesControllerTests
{
    [Fact]
    public async Task Run_QueuedExactSearch_CreatesCandidate()
    {
        await using var database = TestDatabase.Create();
        using var timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var target = new SelectedTargetProjection
        {
            TargetAssessmentId = Guid.NewGuid(),
            TargetHypothesisId = Guid.NewGuid(),
            DiseaseName = "Disease",
            GeneSymbol = "GENE",
            DesiredModulation = "Inhibit",
            Rationale = "Rationale",
            LiteratureEvidenceScore = 0.8m,
            TargetAssessmentScore = 0.8m,
            ConfidenceLevel = "High",
            SourceUpdatedAtUtc = DateTimeOffset.UtcNow,
            SynchronizedAtUtc = DateTimeOffset.UtcNow
        };

        var project = new ChemicalDesignProject
        {
            Id = Guid.NewGuid(),
            TargetAssessmentId = target.TargetAssessmentId,
            Name = "Project",
            DesiredModulation = "Inhibit",
            Status = DesignProjectStatus.InProgress,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        var query = new QueryStructure
        {
            Id = Guid.NewGuid(),
            DesignProjectId = project.Id,
            Name = "Aspirin",
            Structure = "CC(=O)OC1=CC=CC=C1C(=O)O",
            StructureFormat = "SMILES",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        var search = new StructureSearchJob
        {
            Id = Guid.NewGuid(),
            DesignProjectId = project.Id,
            QueryStructureId = query.Id,
            SearchType = StructureSearchType.Exact,
            SimilarityThreshold = 1m,
            MaximumResults = 10,
            Provider = "Prototype",
            Status = StructureSearchStatus.Queued,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        database.Context.SelectedTargets.Add(target);
        database.Context.DesignProjects.Add(project);
        database.Context.QueryStructures.Add(query);
        database.Context.StructureSearchJobs.Add(search);
        database.Context.ChemicalLibrary.Add(
            new ChemicalLibraryEntry
            {
                Id = Guid.NewGuid(),
                Name = "Aspirin",
                CanonicalSmiles = query.Structure,
                MolecularWeight = 180.16m,
                SourceReference = "Test"
            });

        await database.Context.SaveChangesAsync(timeout.Token);
        database.Context.ChangeTracker.Clear();

        var publisher = Substitute.For<IEventPublisher>();

        var controller = new StructureSearchesController(
            database.Context,
            new DeterministicStructureSearchEngine(),
            publisher);

        var action = await controller.Run(search.Id, timeout.Token);

        var ok = action.Result.Should()
            .BeOfType<OkObjectResult>().Subject;

        var response = ok.Value.Should()
            .BeOfType<Contracts.StructureSearchResponse>().Subject;

        response.Status.Should().Be(StructureSearchStatus.Completed);
        response.Candidates.Should().ContainSingle();

        await publisher.Received(1).PublishAsync(
            Arg.Any<StructureSearchCompleted>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}
