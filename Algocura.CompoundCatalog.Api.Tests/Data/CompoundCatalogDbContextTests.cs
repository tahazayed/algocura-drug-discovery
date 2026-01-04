using Algocura.CompoundCatalog.Api.Data;
using Algocura.CompoundCatalog.Api.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Algocura.CompoundCatalog.Api.Tests.Data;

public sealed class CompoundCatalogDbContextTests
    : IAsyncLifetime
{
    private readonly SqliteDatabaseFixture _database = new();

    public async Task InitializeAsync()
    {
        await _database.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
    }

    [Fact]
    public async Task SaveChanges_ShouldPersistCompound()
    {
        await using var context = _database.CreateContext();

        var compound = CompoundSeedData.GetCompounds().First();

        context.Compounds.Add(compound);
        await context.SaveChangesAsync();

        var storedCompound = await context.Compounds
            .AsNoTracking()
            .SingleAsync(x => x.Id == compound.Id);

        storedCompound.Name.Should().Be(compound.Name);
        storedCompound.InchiKey.Should().Be(compound.InchiKey);
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateInchiKey()
    {
        await using var context = _database.CreateContext();

        var original = CompoundSeedData.GetCompounds().First();

        context.Compounds.Add(original);
        await context.SaveChangesAsync();

        context.Compounds.Add(new()
        {
            Id = Guid.NewGuid(),
            Name = "Duplicate compound",
            MolecularFormula = original.MolecularFormula,
            CanonicalSmiles = original.CanonicalSmiles,
            InchiKey = original.InchiKey,
            Status = original.Status,
            CreatedAtUtc = DateTime.UtcNow
        });

        var operation = async () =>
            await context.SaveChangesAsync();

        await operation.Should()
            .ThrowAsync<DbUpdateException>();
    }
}