using Algocura.Search.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Search.Api.Tests.Support;

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    private TestDatabase(
        SqliteConnection connection,
        SearchDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public SearchDbContext Context { get; }

    public static TestDatabase Create()
    {
        var connection =
            new SqliteConnection("Data Source=:memory:");

        connection.Open();

        var options =
            new DbContextOptionsBuilder<SearchDbContext>()
                .UseSqlite(connection)
                .Options;

        var context =
            new SearchDbContext(options);

        context.Database.EnsureCreated();

        return new TestDatabase(
            connection,
            context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}

