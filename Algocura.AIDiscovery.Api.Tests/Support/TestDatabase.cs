using Algocura.AIDiscovery.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Algocura.AIDiscovery.Api.Tests.Support;

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    private TestDatabase(
        SqliteConnection connection,
        AIDiscoveryDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public AIDiscoveryDbContext Context { get; }

    public static TestDatabase Create()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options =
            new DbContextOptionsBuilder<AIDiscoveryDbContext>()
                .UseSqlite(connection)
                .Options;

        var context = new AIDiscoveryDbContext(options);
        context.Database.EnsureCreated();

        return new TestDatabase(connection, context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
