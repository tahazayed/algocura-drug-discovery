using Algocura.LiteratureIntelligence.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Algocura.LiteratureIntelligence.Api.Tests.Support;

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    private TestDatabase(
        SqliteConnection connection,
        LiteratureIntelligenceDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public LiteratureIntelligenceDbContext Context { get; }

    public static TestDatabase Create()
    {
        var connection =
            new SqliteConnection("Data Source=:memory:");

        connection.Open();

        var options =
            new DbContextOptionsBuilder<
                LiteratureIntelligenceDbContext>()
                .UseSqlite(connection)
                .Options;

        var context =
            new LiteratureIntelligenceDbContext(options);

        context.Database.EnsureCreated();

        return new TestDatabase(connection, context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
