using Algocura.ChemicalDesign.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Algocura.ChemicalDesign.Api.Tests.Support;

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    private TestDatabase(
        SqliteConnection connection,
        ChemicalDesignDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public ChemicalDesignDbContext Context { get; }

    public static TestDatabase Create()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ChemicalDesignDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ChemicalDesignDbContext(options);
        context.Database.EnsureCreated();

        return new TestDatabase(connection, context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
