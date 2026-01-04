using Algocura.CompoundCatalog.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Algocura.CompoundCatalog.Api.Tests.Infrastructure;

public sealed class SqliteDatabaseFixture : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteDatabaseFixture()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        Options = new DbContextOptionsBuilder<CompoundCatalogDbContext>()
            .UseSqlite(_connection)
            .EnableDetailedErrors()
            .Options;
    }

    public DbContextOptions<CompoundCatalogDbContext> Options { get; }

    public CompoundCatalogDbContext CreateContext()
    {
        return new CompoundCatalogDbContext(Options);
    }

    public async Task InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task ResetAsync()
    {
        await using var context = CreateContext();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}