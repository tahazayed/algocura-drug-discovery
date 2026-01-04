using Algocura.CompoundCatalog.Api.Controllers;
using Algocura.CompoundCatalog.Api.Data;
using Algocura.CompoundCatalog.Api.Domain;
using Algocura.CompoundCatalog.Api.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Algocura.CompoundCatalog.Api.Tests.Controllers;

public sealed class CompoundsControllerTests
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<CompoundsController> _logger;

    public CompoundsControllerTests()
    {
        _eventPublisher = Substitute.For<IEventPublisher>();
        _logger = Substitute.For<ILogger<CompoundsController>>();
    }

    [Fact]
    public async Task GetAll_ShouldReturnCompounds()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var dbContext = CreateDbContext(connection);

        await dbContext.Database.EnsureCreatedAsync();

        dbContext.Compounds.AddRange(
            CreateCompound(
                Guid.Parse("7f06dc5b-379f-4589-9384-4f09c55f97c3"),
                "Aspirin",
                "BSYNRYMUTXBXSQ-UHFFFAOYSA-N",
                CompoundStatus.Registered),
            CreateCompound(
                Guid.Parse("d2b92d72-5ce1-4659-aa35-9728dab134c6"),
                "Caffeine",
                "RYYVLZVUVIJVGH-UHFFFAOYSA-N",
                CompoundStatus.UnderEvaluation));

        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext);

        var result = await controller.GetAll(
            search: null,
            status: null,
            skip: 0,
            take: 50,
            cancellationToken: CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var compounds = Assert.IsAssignableFrom<
            IReadOnlyCollection<Contracts.CompoundResponse>>(
            okResult.Value);

        Assert.Equal(2, compounds.Count);
    }

    [Fact]
    public async Task GetAll_ShouldFilterCompoundsByStatus()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var dbContext = CreateDbContext(connection);

        await dbContext.Database.EnsureCreatedAsync();

        dbContext.Compounds.AddRange(
            CreateCompound(
                Guid.NewGuid(),
                "Aspirin",
                "BSYNRYMUTXBXSQ-UHFFFAOYSA-N",
                CompoundStatus.Registered),
            CreateCompound(
                Guid.NewGuid(),
                "Caffeine",
                "RYYVLZVUVIJVGH-UHFFFAOYSA-N",
                CompoundStatus.UnderEvaluation));

        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext);

        var result = await controller.GetAll(
            search: null,
            status: CompoundStatus.Registered,
            skip: 0,
            take: 50,
            cancellationToken: CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var compounds = Assert.IsAssignableFrom<
            IReadOnlyCollection<Contracts.CompoundResponse>>(
            okResult.Value);

        var compound = Assert.Single(compounds);

        Assert.Equal("Aspirin", compound.Name);
        Assert.Equal(CompoundStatus.Registered, compound.Status);
    }

    [Fact]
    public async Task GetAll_ShouldFilterCompoundsBySearchText()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var dbContext = CreateDbContext(connection);

        await dbContext.Database.EnsureCreatedAsync();

        dbContext.Compounds.AddRange(
            CreateCompound(
                Guid.NewGuid(),
                "Aspirin",
                "BSYNRYMUTXBXSQ-UHFFFAOYSA-N",
                CompoundStatus.Registered),
            CreateCompound(
                Guid.NewGuid(),
                "Caffeine",
                "RYYVLZVUVIJVGH-UHFFFAOYSA-N",
                CompoundStatus.UnderEvaluation));

        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext);

        var result = await controller.GetAll(
            search: "caffeine",
            status: null,
            skip: 0,
            take: 50,
            cancellationToken: CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var compounds = Assert.IsAssignableFrom<
            IReadOnlyCollection<Contracts.CompoundResponse>>(
            okResult.Value);

        var compound = Assert.Single(compounds);

        Assert.Equal("Caffeine", compound.Name);
    }

    [Fact]
    public async Task GetById_ShouldReturnCompound_WhenCompoundExists()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var dbContext = CreateDbContext(connection);

        await dbContext.Database.EnsureCreatedAsync();

        var compoundId =
            Guid.Parse("7f06dc5b-379f-4589-9384-4f09c55f97c3");

        dbContext.Compounds.Add(
            CreateCompound(
                compoundId,
                "Aspirin",
                "BSYNRYMUTXBXSQ-UHFFFAOYSA-N",
                CompoundStatus.Registered));

        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext);

        var result = await controller.GetById(
            compoundId,
            CancellationToken.None);

        var okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        var response =
            Assert.IsType<Contracts.CompoundResponse>(okResult.Value);

        Assert.Equal(compoundId, response.Id);
        Assert.Equal("Aspirin", response.Name);
    }

    [Fact]
    public async Task GetById_ShouldReturn404_WhenCompoundDoesNotExist()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        await using var dbContext = CreateDbContext(connection);

        await dbContext.Database.EnsureCreatedAsync();

        var controller = CreateController(dbContext);

        var result = await controller.GetById(
            Guid.NewGuid(),
            CancellationToken.None);

        var objectResult =
            Assert.IsType<ObjectResult>(result.Result);

        Assert.Equal(
            StatusCodes.Status404NotFound,
            objectResult.StatusCode);

        var problemDetails =
            Assert.IsType<ProblemDetails>(objectResult.Value);

        Assert.Equal("Compound not found", problemDetails.Title);
    }

    private CompoundsController CreateController(
        CompoundCatalogDbContext dbContext)
    {
        var controller = new CompoundsController(
            dbContext,
            _eventPublisher,
            _logger);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                TraceIdentifier = "compound-controller-test"
            }
        };

        return controller;
    }

    private static CompoundCatalogDbContext CreateDbContext(
        SqliteConnection connection)
    {
        var options =
            new DbContextOptionsBuilder<CompoundCatalogDbContext>()
                .UseSqlite(connection)
                .Options;

        return new CompoundCatalogDbContext(options);
    }

    private static Compound CreateCompound(
        Guid id,
        string name,
        string inchiKey,
        CompoundStatus status)
    {
        var timestamp = DateTimeOffset.UtcNow;

        return new Compound
        {
            Id = id,
            Name = name,
            CanonicalSmiles = name == "Aspirin"
                ? "CC(=O)OC1=CC=CC=C1C(=O)O"
                : "CN1C=NC2=C1C(=O)N(C(=O)N2C)C",
            InchiKey = inchiKey,
            MolecularWeight = name == "Aspirin"
                ? 180.16m
                : 194.19m,
            MolecularFormula = name == "Aspirin"
                ? "C9H8O4"
                : "C8H10N4O2",
            Source = "Controller unit test",
            Description = $"Test compound: {name}.",
            Status = status,
            CreatedAtUtc = timestamp,
            UpdatedAtUtc = timestamp
        };
    }
}