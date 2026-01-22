using Algocura.AIDiscovery.Api.Contracts;
using Algocura.AIDiscovery.Api.Controllers;
using Algocura.AIDiscovery.Api.Domain;
using Algocura.AIDiscovery.Api.Events;
using Algocura.AIDiscovery.Api.Routing;
using Algocura.AIDiscovery.Api.Services;
using Algocura.AIDiscovery.Api.Tests.Support;
using Algocura.Platform.Messaging.Publishing;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Algocura.AIDiscovery.Api.Tests.Controllers;

public sealed class PredictionsControllerTests
{
    [Fact]
    public async Task Create_WithKnownCompound_CreatesQueuedPrediction()
    {
        await using var database = TestDatabase.Create();
        using var timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var model = CreateModel();
        var compound = new CompoundProjection
        {
            CompoundId = Guid.NewGuid(),
            Name = "Aspirin",
            CanonicalSmiles = "SMILES",
            MolecularWeight = 180.16m,
            Status = 2,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        database.Context.ModelDefinitions.Add(model);
        database.Context.Compounds.Add(compound);
        await database.Context.SaveChangesAsync(timeout.Token);
        database.Context.ChangeTracker.Clear();

        var publisher = Substitute.For<IEventPublisher>();

        var sut = new PredictionsController(
            database.Context,
            new DeterministicPredictionScoringEngine(),
            Substitute.For<IAdaptivePredictionOrchestrator>(),
            publisher,
            NullLogger<PredictionsController>.Instance);

        var action = await sut.Create(
            new CreatePredictionRequest(
                "Prediction",
                model.Id,
                null,
                null,
                [compound.CompoundId]),
            timeout.Token);

        var created = action.Result.Should()
            .BeOfType<CreatedAtActionResult>()
            .Subject;

        var response = created.Value.Should()
            .BeOfType<PredictionResponse>()
            .Subject;

        response.Status.Should().Be(PredictionStatus.Queued);

        await publisher.Received(1).PublishAsync(
            Arg.Any<PredictionCreated>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Run_QueuedPrediction_CompletesAndRanksResults()
    {
        await using var database = TestDatabase.Create();
        using var timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var model = CreateModel();

        var job = new PredictionJob
        {
            Id = Guid.NewGuid(),
            Name = "Prediction",
            ModelDefinitionId = model.Id,
            Status = PredictionStatus.Queued,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            Candidates =
            [
                new PredictionCandidate
                {
                    Id = Guid.NewGuid(),
                    CompoundId = Guid.NewGuid(),
                    CompoundName = "A",
                    ScreeningScore = 30m,
                    MolecularWeight = 250m
                },
                new PredictionCandidate
                {
                    Id = Guid.NewGuid(),
                    CompoundId = Guid.NewGuid(),
                    CompoundName = "B",
                    ScreeningScore = 90m,
                    MolecularWeight = 250m,
                    ScreeningSelected = true
                }
            ]
        };

        database.Context.ModelDefinitions.Add(model);
        database.Context.PredictionJobs.Add(job);
        await database.Context.SaveChangesAsync(timeout.Token);
        database.Context.ChangeTracker.Clear();

        var publisher = Substitute.For<IEventPublisher>();

        var sut = new PredictionsController(
            database.Context,
            new DeterministicPredictionScoringEngine(),
            Substitute.For<IAdaptivePredictionOrchestrator>(),
            publisher,
            NullLogger<PredictionsController>.Instance);

        var action = await sut.Run(job.Id, timeout.Token);

        var ok = action.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = ok.Value.Should()
            .BeOfType<PredictionResponse>()
            .Subject;

        response.Status.Should().Be(PredictionStatus.Completed);
        response.Results.Should().HaveCount(2);

        await publisher.Received(1).PublishAsync(
            Arg.Any<PredictionCompleted>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    private static ModelDefinition CreateModel()
    {
        return new ModelDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Model",
            Version = "1.0",
            ScreeningScoreWeight = 0.75m,
            MolecularWeightWeight = 0.15m,
            SelectionBonus = 10m,
            Status = ModelStatus.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
    }
}
